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

using System;
using System.IO;
using System.Text;

namespace OdinSdk.eTaxBill.Net.Mime
{
    /// <summary>
    /// Mime constructor.
    /// </summary>
    public class MimeConstructor
    {
        private string m_messageId = "";

        private string m_from = "";
        private string[] m_to = null;
        private string[] m_cc = null;
        private string[] m_bcc = null;

        private string m_subject = "";
        private string m_bodyText = "";
        private string m_bodyHtml = "";

        private string m_charSet = "";

        private DateTime m_messageDate;
        private Attachments m_attachments = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MimeConstructor()
        {
            m_attachments = new Attachments();

            m_messageDate = DateTime.Now;
            m_messageId = "<" + Guid.NewGuid().ToString().Replace("-", "") + ">";
            m_charSet = "utf-8";
        }

        /// <summary>
        /// Constructs mime.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ConstructBinaryMime()
        {
            var _mime = new MemoryStream();
            byte[] _buffer = null;

            var _mainBoundary = "----=_NextPart_" + Guid.NewGuid().ToString().Replace("-", "_");
            {
                // Message-ID:
                _buffer = Encoding.UTF8.GetBytes("Message-ID: " + m_messageId + "\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                // From:
                _buffer = Encoding.UTF8.GetBytes("From: " + CEnCode(m_from) + "\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                // To:
                _buffer = Encoding.UTF8.GetBytes("To: " + this.ConstructAddress(m_to ?? new string[0]));
                _mime.Write(_buffer, 0, _buffer.Length);

                // Cc:
                if (m_cc != null && m_cc.Length > 0)
                {
                    _buffer = Encoding.UTF8.GetBytes("Cc: " + this.ConstructAddress(m_cc));
                    _mime.Write(_buffer, 0, _buffer.Length);
                }

                // Bcc:
                if (m_bcc != null && m_bcc.Length > 0)
                {
                    _buffer = Encoding.UTF8.GetBytes("Bcc: " + this.ConstructAddress(m_bcc));
                    _mime.Write(_buffer, 0, _buffer.Length);
                }

                // Subject:
                _buffer = Encoding.UTF8.GetBytes("Subject: " + CEnCode(m_subject) + "\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                // Date:
                _buffer = Encoding.UTF8.GetBytes("Date: " + m_messageDate.ToUniversalTime().ToString("r", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                // MIME-Version:
                _buffer = Encoding.UTF8.GetBytes("MIME-Version: 1.0\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                // Content-Type:
                _buffer = Encoding.UTF8.GetBytes("Content-Type: " + "multipart/mixed;\r\n\tboundary=\"" + _mainBoundary + "\"\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                //
                _buffer = Encoding.UTF8.GetBytes("\r\nThis is a multi-part message in MIME format.\r\n\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);
            }

            var _bodyBoundary = "----=_NextPart_" + Guid.NewGuid().ToString().Replace("-", "_");
            {
                _buffer = Encoding.UTF8.GetBytes("--" + _mainBoundary + "\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                _buffer = Encoding.UTF8.GetBytes("Content-Type: multipart/alternative;\r\n\tboundary=\"" + _bodyBoundary + "\"\r\n\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                _buffer = Encoding.UTF8.GetBytes(ConstructBody(_bodyBoundary));
                _mime.Write(_buffer, 0, _buffer.Length);

                _buffer = Encoding.UTF8.GetBytes("--" + _bodyBoundary + "--\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                //-- Construct attachments
                foreach (Attachment _attach in m_attachments)
                {
                    _buffer = Encoding.UTF8.GetBytes("\r\n--" + _mainBoundary + "\r\n");
                    _mime.Write(_buffer, 0, _buffer.Length);

                    _buffer = Encoding.UTF8.GetBytes("Content-Type: application/octet;\r\n\tname=\"" + _attach.FileName + "\"\r\n");
                    _mime.Write(_buffer, 0, _buffer.Length);

                    _buffer = Encoding.UTF8.GetBytes("Content-Transfer-Encoding: base64\r\n");
                    _mime.Write(_buffer, 0, _buffer.Length);

                    _buffer = Encoding.UTF8.GetBytes("Content-Disposition: attachment;\r\n\tfilename=\"" + _attach.FileName + "\"\r\n\r\n");
                    _mime.Write(_buffer, 0, _buffer.Length);

                    _buffer = Encoding.UTF8.GetBytes(SplitString(Convert.ToBase64String(_attach.FileData)));
                    _mime.Write(_buffer, 0, _buffer.Length);
                }

                _buffer = Encoding.UTF8.GetBytes("\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);

                _buffer = Encoding.UTF8.GetBytes("--" + _mainBoundary + "--\r\n");
                _mime.Write(_buffer, 0, _buffer.Length);
            }

            _mime.Position = 0;
            return _mime;
        }

        #region function ConstructMime

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string ConstructMime()
        {
            return Encoding.UTF8.GetString(this.ConstructBinaryMime().ToArray());

            /*
            var _builder = new StringBuilder();
            var _mainBoundary = "----=_NextPart_" + Guid.NewGuid().ToString().Replace("-", "_");

            _builder.Append("Message-ID: " + m_MsgID + "\r\n");
            _builder.Append("From: " + CEnCode(m_From) + "\r\n");
            _builder.Append("To: " + this.ConstructAddress(m_To));

            if (m_Cc != null && m_Cc.Length > 0)
            {
                _builder.Append("Cc: " + this.ConstructAddress(m_Cc));
            }

            if (m_Bcc != null && m_Bcc.Length > 0)
            {
                _builder.Append("Bcc: " + this.ConstructAddress(m_Bcc));
            }

            _builder.Append("Subject: " + CEnCode(m_Subject) + "\r\n");
            _builder.Append("Date: " + m_MsgDate.ToUniversalTime().ToString("r", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n");
            _builder.Append("MIME-Version: 1.0\r\n");
            _builder.Append("Content-Type: " + "multipart/mixed;\r\n");
            _builder.Append("\tboundary=\"" + _mainBoundary + "\"\r\n");

            _builder.Append("\r\nThis is a multi-part message in MIME format.\r\n\r\n");

            //----
            if (m_pAttachments.Count == 0)
            {
                _builder.Append(ConstructBody(_mainBoundary));
            }
            else
            {
                var bodyBoundaryID = "----=_NextPart_" + Guid.NewGuid().ToString().Replace("-", "_");
                _builder.Append("--" + _mainBoundary + "\r\n");
                _builder.Append("Content-Type: multipart/alternative;\r\n");
                _builder.Append("\tboundary=\"" + bodyBoundaryID + "\"\r\n\r\n");

                _builder.Append(ConstructBody(bodyBoundaryID));

                _builder.Append("--" + bodyBoundaryID + "--\r\n");

                //-- Construct attachments
                foreach (Attachment att in m_pAttachments)
                {
                    _builder.Append("\r\n");
                    _builder.Append("--" + _mainBoundary + "\r\n");
                    _builder.Append("Content-Type: application/octet;\r\n");
                    _builder.Append("\tname=\"" + att.FileName + "\"\r\n");
                    _builder.Append("Content-Transfer-Encoding: base64\r\n");
                    _builder.Append("Content-Disposition: attachment;\r\n");
                    _builder.Append("\tfilename=\"" + att.FileName + "\"\r\n\r\n");

                    _builder.Append(SplitString(Convert.ToBase64String(att.FileData)));
                }

                _builder.Append("\r\n");
            }

            _builder.Append("--" + _mainBoundary + "--\r\n");

            return _builder.ToString();
            */
        }

        #endregion function ConstructMime

        #region function ConstructBody

        private string ConstructBody(string boundaryID)
        {
            var _builder = new StringBuilder();

            _builder.Append("--" + boundaryID + "\r\n");
            _builder.Append("Content-Type: text/plain;\r\n");
            _builder.Append("\tcharset=\"" + m_charSet + "\"\r\n");
            _builder.Append("Content-Transfer-Encoding: base64\r\n\r\n");

            _builder.Append(this.SplitString(Convert.ToBase64String(Encoding.GetEncoding(m_charSet).GetBytes(this.Body)) + "\r\n\r\n"));

            // We have html body, construct it.
            if (this.BodyHtml.Length > 0)
            {
                _builder.Append("--" + boundaryID + "\r\n");
                _builder.Append("Content-Type: text/html;\r\n");
                _builder.Append("\tcharset=\"" + m_charSet + "\"\r\n");
                _builder.Append("Content-Transfer-Encoding: base64\r\n\r\n");

                _builder.Append(SplitString(Convert.ToBase64String(Encoding.GetEncoding(m_charSet).GetBytes(this.BodyHtml))));
            }

            return _builder.ToString();
        }

        #endregion function ConstructBody

        #region function ConstructAddress

        private string ConstructAddress(string[] address)
        {
            if (address != null && address.Length > 0)
            {
                if (address.Length > 1)
                {
                    var to = "";
                    for (int i = 0; i < address.Length; i++)
                    {
                        if ((address.Length - i) > 1)
                        {
                            to += CEnCode(address[i]) + ",\r\n\t";
                        }
                        else
                        {
                            to += CEnCode(address[i]) + "\r\n";
                        }
                    }

                    return to;
                }
                else
                {
                    return CEnCode(address[0]) + "\r\n";
                }
            }

            return "";
        }

        #endregion function ConstructAddress

        #region function CEnCode

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string CEnCode(string str)
        {
            var _result = str;

            var _needConvert = false;
            foreach (char c in str)
            {
                // Contains non ascii chars
                if (c > 127)
                {
                    _needConvert = true;
                    break;
                }
            }

            if (_needConvert)
            {
                _result = "=?" + m_charSet + "?" + "B?";
                _result += Convert.ToBase64String(Encoding.GetEncoding(m_charSet).GetBytes(str));
                _result += "?=";
            }

            return _result;
        }

        #endregion function CEnCode

        #region function SplitString

        private string SplitString(string _splitLen)
        {
            var _builder = new StringBuilder();

            var _length = _splitLen.Length;
            var _position = 0;
            while (_position < _length)
            {
                if ((_length - _position) > 76)
                {
                    _builder.Append(_splitLen.Substring(_position, 76) + "\r\n");
                }
                else
                {
                    _builder.Append(_splitLen.Substring(_position, _splitLen.Length - _position) + "\r\n");
                }

                _position += 76;
            }

            return _builder.ToString();
        }

        #endregion function SplitString

        #region Properties Implementation

        /// <summary>
        /// Gets or sets mesaage ID.
        /// </summary>
        public string MessageID
        {
            get
            {
                return m_messageId;
            }

            set
            {
                m_messageId = value;
            }
        }

        /// <summary>
        /// Gets or sets receptients.
        /// </summary>
        public string[] To
        {
            get
            {
                return m_to;
            }

            set
            {
                m_to = value;
            }
        }

        /// <summary>
        /// Gets or sets .
        /// </summary>
        public string[] Cc
        {
            get
            {
                return m_cc;
            }

            set
            {
                m_cc = value;
            }
        }

        /// <summary>
        /// Gets or sets .
        /// </summary>
        public string[] Bcc
        {
            get
            {
                return m_bcc;
            }

            set
            {
                m_bcc = value;
            }
        }

        /// <summary>
        /// Gets or sets sender.
        /// </summary>
        public string From
        {
            get
            {
                return m_from;
            }

            set
            {
                m_from = value;
            }
        }

        /// <summary>
        /// Gets or sets subject.
        /// </summary>
        public string Subject
        {
            get
            {
                return m_subject;
            }

            set
            {
                m_subject = value;
            }
        }

        /// <summary>
        /// Gets or sets message date.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return m_messageDate;
            }

            set
            {
                m_messageDate = value;
            }
        }

        /// <summary>
        /// Gets or sets body text.
        /// </summary>
        public string Body
        {
            get
            {
                return m_bodyText;
            }

            set
            {
                m_bodyText = value;
            }
        }

        /// <summary>
        /// Gets or sets html body.
        /// </summary>
        public string BodyHtml
        {
            get
            {
                return m_bodyHtml;
            }

            set
            {
                m_bodyHtml = value;
            }
        }

        /// <summary>
        /// Gets or sets message charset. Default is 'iso-8859-1'.
        /// </summary>
        public string CharSet
        {
            get
            {
                return m_charSet;
            }

            set
            {
                m_charSet = value;
            }
        }

        /// <summary>
        /// Gets referance to attachments collection.
        /// </summary>
        public Attachments Attachments
        {
            get
            {
                return m_attachments;
            }
        }

        #endregion Properties Implementation
    }
}