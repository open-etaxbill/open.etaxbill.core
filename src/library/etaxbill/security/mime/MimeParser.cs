/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using OdinSdk.eTaxBill.Utility;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OdinSdk.eTaxBill.Security.Mime
{
    /// <summary>
    ///
    /// </summary>
    public class MimeParser
    {
        /// <summary>
        ///
        /// </summary>
        public byte[] CarriageReturnLineFeed = new byte[] { (byte)'\r', (byte)'\n' };

        /// <summary>
        ///
        /// </summary>
        public byte[] EndOfPartsDelimeter = new byte[] { (byte)'-', (byte)'-' };

        /// <summary>
        ///
        /// </summary>
        public Encoding ParserEncoding
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public Encoding SoapXmlEncoding
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public MimeParser()
        {
            ParserEncoding = Encoding.UTF8;
            SoapXmlEncoding = Encoding.UTF8;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="encoding"></param>
        public MimeParser(Encoding encoding)
            : this()
        {
            ParserEncoding = encoding;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="soap_encoding"></param>
        public MimeParser(Encoding encoding, Encoding soap_encoding)
            : this(encoding)
        {
            SoapXmlEncoding = soap_encoding;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mime_content"></param>
        /// <returns></returns>
        public byte[] SerializeMimeContent(MimeContent mime_content)
        {
            MemoryStream _contentStream = new MemoryStream();
            this.SerializeMimeContent(mime_content, _contentStream);
            return _contentStream.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="in_content"></param>
        /// <param name="out_stream"></param>
        public void SerializeMimeContent(MimeContent in_content, Stream out_stream)
        {
            byte[] _writeHelper;

            //
            // Prepare some bytes written more than once
            //
            byte[] _boundaryBytes = ParserEncoding.GetBytes("--" + in_content.Boundary);

            //
            // Write every part into the stream
            //
            foreach (var item in in_content.Parts)
            {
                //
                // First of all write the boundary
                //
                out_stream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
                out_stream.Write(_boundaryBytes, 0, _boundaryBytes.Length);
                out_stream.Write(CarriageReturnLineFeed, 0, 2);

                //
                // Write the content-type for the current element
                //
                var _builder = new StringBuilder();
                _builder.Append(String.Format("Content-Type: {0}", item.ContentType));
                if (!String.IsNullOrEmpty(item.CharSet))
                    _builder.Append(String.Format("; charset={0}", item.CharSet));
                _builder.Append(new char[] { '\r', '\n' });

                if (!String.IsNullOrEmpty(item.TransferEncoding))
                {
                    _builder.Append(String.Format("Content-Transfer-Encoding: {0}", item.TransferEncoding));
                    _builder.Append(new char[] { '\r', '\n' });
                }

                _builder.Append(String.Format("Content-ID: {0}", item.ContentId));

                _writeHelper = ParserEncoding.GetBytes(_builder.ToString());
                out_stream.Write(_writeHelper, 0, _writeHelper.Length);

                out_stream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
                out_stream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);

                //
                // Write the actual content
                //
                out_stream.Write(item.Content, 0, item.Content.Length);
            }

            //
            // Write one last content boundary
            //
            out_stream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
            out_stream.Write(_boundaryBytes, 0, _boundaryBytes.Length);
            out_stream.Write(EndOfPartsDelimeter, 0, EndOfPartsDelimeter.Length);
            out_stream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="http_content_type"></param>
        /// <param name="binary_content"></param>
        /// <returns></returns>
        public MimeContent DeserializeMimeContent(string http_content_type, byte[] binary_content)
        {
            //
            // First of all parse the http content type
            //
            string _mimeType = null, _mimeBoundary = null, _mimeStart = null;
            ParseHttpContentTypeHeader(http_content_type, ref _mimeType, ref _mimeBoundary, ref _mimeStart);

            //
            // Create the mime-content
            //
            MimeContent _content = new MimeContent()
            {
                Boundary = _mimeBoundary
            };

            //
            // Start finding the parts in the mime message
            // Note: in MIME RFC a "--" represents the end of something
            //
            var _endBoundaryHelperNdx = 0;
            byte[] _mimeBoundaryBytes = ParserEncoding.GetBytes("--" + _mimeBoundary);
            for (int i = 0; i < binary_content.Length; i++)
            {
                if (AreArrayPartsForTextEqual(_mimeBoundaryBytes, 0, binary_content, i, _mimeBoundaryBytes.Length))
                {
                    _endBoundaryHelperNdx = i + _mimeBoundaryBytes.Length;
                    if ((_endBoundaryHelperNdx + 1) < binary_content.Length)
                    {
                        // The end of the MIME-message is the boundary followed by "--"
                        if (binary_content[_endBoundaryHelperNdx] == '-' && binary_content[_endBoundaryHelperNdx + 1] == '-')
                        {
                            break;
                        }
                    }
                    else
                    {
                        throw new ProxyException("Invalid MIME content parsed, premature End-Of-File detected!");
                    }

                    // Start reading the mime part after the boundary
                    MimePart _part = ReadMimePart(binary_content, ref i, _mimeBoundaryBytes);
                    if (_part != null)
                    {
                        _content.Parts.Add(_part);
                    }
                }
            }

            //
            // Finally return the ready-to-use object model
            //
            _content.SetAsStartPart(_mimeStart);
            return _content;
        }

        private void ParseHttpContentTypeHeader(string http_content_type, ref string mime_type, ref string mime_boundary, ref string mime_start)
        {
            string[] _contentTypeParsed = http_content_type.Split(new char[] { ';' });
            for (int i = 0; i < _contentTypeParsed.Length; i++)
            {
                var _contentTypePart = _contentTypeParsed[i].Trim();

                var _equalsNdx = _contentTypePart.IndexOf('=');
                if (_equalsNdx <= 0)
                    continue;

                var _key = _contentTypePart.Substring(0, _equalsNdx);
                var _value = _contentTypePart.Substring(_equalsNdx + 1);
                if (_value[0] == '\"')
                    _value = _value.Remove(0, 1);
                if (_value[_value.Length - 1] == '\"')
                    _value = _value.Remove(_value.Length - 1);

                switch (_key.ToLower())
                {
                    case "type":
                        mime_type = _value;
                        break;

                    case "start":
                        mime_start = _value;
                        break;

                    case "boundary":
                        mime_boundary = _value;
                        break;
                }
            }

            if ((mime_type == null) || (mime_start == null) || (mime_boundary == null))
            {
                throw new ProxyException("Invalid HTTP content header - please verify if type, start and boundary are available in the multipart/related content type header!");
            }
        }

        private MimePart ReadMimePart(byte[] binary_content, ref int current_index, byte[] mime_boundary_bytes)
        {
            byte[] _contentTypeKeyBytes = ParserEncoding.GetBytes("Content-Type:");
            byte[] _transferEncodingKeyBytes = ParserEncoding.GetBytes("Content-Transfer-Encoding:");
            byte[] _contentIdKeyBytes = ParserEncoding.GetBytes("Content-ID:");

            //
            // Find the appropriate content header indexes
            //
            int _contentTypeNdx = -1, _transferEncodingNdx = -1, _contentIdNdx = -1;
            int _contentTypeLen = -1, _transferEncodingLen = -1, _contentIdLen = -1;
            while (current_index < binary_content.Length)
            {
                // Try compare for keys
                if (_contentTypeNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_contentTypeKeyBytes, 0, binary_content, current_index, _contentTypeKeyBytes.Length) == true)
                    {
                        _contentTypeNdx = current_index;
                        _contentTypeLen = this.GetLengthToCRLF(binary_content, _contentTypeNdx + _contentTypeKeyBytes.Length);
                    }
                }

                if (_transferEncodingNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_transferEncodingKeyBytes, 0, binary_content, current_index, _transferEncodingKeyBytes.Length) == true)
                    {
                        _transferEncodingNdx = current_index;
                        _transferEncodingLen = this.GetLengthToCRLF(binary_content, _transferEncodingNdx + _transferEncodingKeyBytes.Length);
                    }
                }

                if (_contentIdNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_contentIdKeyBytes, 0, binary_content, current_index, _contentIdKeyBytes.Length) == true)
                    {
                        _contentIdNdx = current_index;
                        _contentIdLen = this.GetLengthToCRLF(binary_content, _contentIdNdx + _contentIdKeyBytes.Length);
                    }
                }

                // All content headers found, last content header split by Carriage Return Line Feed
                // TODO: Check index out of bounds!
                if (binary_content[current_index] == 13 && binary_content[current_index + 1] == 10)
                {
                    if (binary_content[current_index + 2] == 13 && binary_content[current_index + 3] == 10)
                        break;
                }

                // Next array index
                current_index++;
            }

            // After the last content header, we have \r\n\r\n, always
            current_index += 4;

            //
            // If not all indices found, error
            //
            //if (!((_contentTypeNdx >= 0) && (_transferEncodingNdx >= 0) && (_contentIdNdx >= 0)))
            //{
            //    // A '0' at the end of the message indicates that the previous part was the last one
            //    if (binary_content[current_index - 1] == 0)
            //        return null;
            //    else if (binary_content[current_index - 2] == 13 && binary_content[current_index - 1] == 10)
            //        return null;
            //    else
            //        throw new ProxyException("Invalid mime content passed into mime parser! Content-Type, Content-Transfer-Encoding or ContentId headers for mime part are missing!");
            //}

            //
            // Convert the content header information into strings
            //
            var _contentType = "";
            var _charSet = "";

            if (_contentTypeNdx > 0)
            {
                _contentType = ParserEncoding.GetString(binary_content, _contentTypeNdx + _contentTypeKeyBytes.Length, _contentTypeLen).Trim();

                if (_contentType.Contains(';'))
                {
                    var _contentTypeSplitIdx = _contentType.IndexOf(';');
                    var _equalsCharSetNdx = _contentType.IndexOf('=', _contentTypeSplitIdx + 1);

                    if (_equalsCharSetNdx < 0)
                        _charSet = _contentType.Substring(_contentTypeSplitIdx + 1).Trim();
                    else
                        _charSet = _contentType.Substring(_equalsCharSetNdx + 1).Trim();

                    _contentType = _contentType.Substring(0, _contentTypeSplitIdx).Trim();
                }
            }

            var _transferEncoding = "";
            if (_transferEncodingNdx > 0)
            {
                _transferEncoding = ParserEncoding.GetString(binary_content, _transferEncodingNdx + _transferEncodingKeyBytes.Length, _transferEncodingLen).Trim();
            }

            var _contentId = "";
            if (_contentIdNdx > 0)
            {
                _contentId = ParserEncoding.GetString(binary_content, _contentIdNdx + _contentIdKeyBytes.Length, _contentIdLen).Trim();
            }

            //
            // Current mime content starts now, therefore find the end
            //
            var _startContentIndex = current_index;
            var _endContentIndex = -1;
            while (current_index < binary_content.Length)
            {
                if (AreArrayPartsForTextEqual(mime_boundary_bytes, 0, binary_content, current_index, mime_boundary_bytes.Length))
                {
                    _endContentIndex = current_index - 1;
                    break;
                }

                current_index++;
            }
            if (_endContentIndex == -1)
                _endContentIndex = current_index - 1;

            //
            // Tweak start- and end-indexes, cut all Carriage Return Line Feeds
            //
            while (true)
            {
                if ((binary_content[_startContentIndex] == 13) && (binary_content[_startContentIndex + 1] == 10))
                    _startContentIndex += 2;
                else
                    break;

                if (_startContentIndex > binary_content.Length)
                    throw new ProxyException("Error in content, start index cannot go beyond overall content array!");
            }

            while (true)
            {
                if ((binary_content[_endContentIndex - 1] == 13) && (binary_content[_endContentIndex] == 10))
                    _endContentIndex -= 2;
                else
                    break;

                if (_endContentIndex < 0)
                    throw new ProxyException("Error in content, end content index cannot go beyond smallest index of content array!");
            }

            //
            // Now create a byte array for the current mime-part content
            //
            MimePart _mimePart = new MimePart()
            {
                ContentId = _contentId,
                TransferEncoding = _transferEncoding,
                ContentType = _contentType,
                CharSet = _charSet,
                Content = new byte[_endContentIndex - _startContentIndex + 1]
            };

            Array.Copy(binary_content, _startContentIndex, _mimePart.Content, 0, _mimePart.Content.Length);

            // Go to the last sign before the next boundary starts
            current_index--;

            return _mimePart;
        }

        private bool AreArrayPartsForTextEqual(byte[] first_array, int first_offset, byte[] second_array, int second_offset, int length)
        {
            var _result = false;

            // Check array boundaries
            if ((first_offset + length) <= first_array.Length && (second_offset + length) <= second_array.Length)
            {
                _result = true;

                // Run through the arrays and compare byte-by-byte
                for (int i = 0; i < length; i++)
                {
                    char c1 = Char.ToLower((char)first_array[first_offset + i]);
                    char c2 = Char.ToLower((char)second_array[second_offset + i]);
                    if (c1 != c2)
                    {
                        _result = false;
                        break;
                    }
                }
            }

            return _result;
        }

        private int GetLengthToCRLF(byte[] byte_arrary, int offset)
        {
            var _result = 0;

            for (int i = offset; i < byte_arrary.Length; i++)
            {
                if (byte_arrary[i] == 13 && byte_arrary[i + 1] == 10)
                    break;

                _result++;
            }

            return _result;
        }
    }
}