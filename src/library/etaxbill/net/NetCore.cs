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

using OdinSdk.eTaxBill.Net.Core;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OdinSdk.eTaxBill.Net
{
    #region enum AuthType

    /// <summary>
    /// Authentication type.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Plain username/password authentication.
        /// </summary>
        Plain = 0,

        /// <summary>
        /// APOP
        /// </summary>
        APOP = 1,

        /// <summary>
        /// Not implemented.
        /// </summary>
        LOGIN = 2,

        /// <summary>
        /// Cram-md5 authentication.
        /// </summary>
        CRAM_MD5 = 3,
    }

    #endregion enum AuthType

    /// <summary>
    /// Provides net core utility methods.
    /// </summary>
    public class NetCore
    {
        #region function DoPeriodHandling

        /// <summary>
        /// Does period handling.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="add_Remove">If true add periods, else removes periods.</param>
        /// <returns></returns>
        public MemoryStream DoPeriodHandling(byte[] data, bool add_Remove)
        {
            using (MemoryStream strm = new MemoryStream(data))
            {
                return DoPeriodHandling(strm, add_Remove);
            }
        }

        /// <summary>
        /// Does period handling.
        /// </summary>
        /// <param name="strm">Input stream.</param>
        /// <param name="add_Remove">If true add periods, else removes periods.</param>
        /// <returns></returns>
        public MemoryStream DoPeriodHandling(Stream strm, bool add_Remove)
        {
            return DoPeriodHandling(strm, add_Remove, true);
        }

        /// <summary>
        /// Does period handling.
        /// </summary>
        /// <param name="strm">Input stream.</param>
        /// <param name="add_Remove">If true add periods, else removes periods.</param>
        /// <param name="setStrmPosTo0">If true sets stream position to 0.</param>
        /// <returns></returns>
        public MemoryStream DoPeriodHandling(Stream strm, bool add_Remove, bool setStrmPosTo0)
        {
            MemoryStream replyData = new MemoryStream();

            byte[] crlf = new byte[] { (byte)'\r', (byte)'\n' };

            if (setStrmPosTo0)
            {
                strm.Position = 0;
            }

            byte[] line = this.StreamReadLine(strm);

            // Loop through all lines
            while (line != null)
            {
                if (line.Length > 0)
                {
                    if (line[0] == (byte)'.')
                    {
                        /* Add period Rfc 281 4.5.2
                           -  Before sending a line of mail text, the SMTP client checks the
                           first character of the line.  If it is a period, one additional
                           period is inserted at the beginning of the line.
                        */
                        if (add_Remove)
                        {
                            replyData.WriteByte((byte)'.');
                            replyData.Write(line, 0, line.Length);
                        }
                        /* Remove period Rfc 281 4.5.2
                         If the first character is a period , the first characteris deleted.
                        */
                        else
                        {
                            replyData.Write(line, 1, line.Length - 1);
                        }
                    }
                    else
                    {
                        replyData.Write(line, 0, line.Length);
                    }
                }

                replyData.Write(crlf, 0, crlf.Length);

                // Read next line
                line = this.StreamReadLine(strm);
            }

            replyData.Position = 0;

            return replyData;
        }

        #endregion function DoPeriodHandling

        #region function StreamReadLine

        /// <summary>
        /// Reads byte[] line from stream.
        /// </summary>
        /// <returns>Return null if end of stream reached.</returns>
        public byte[] StreamReadLine(Stream stream_source)
        {
            byte[] _result = null;

            ArrayList _lineBuffer = new ArrayList();
            byte _prevByte = 0;

            var _currByteLength = stream_source.ReadByte();
            while (_currByteLength > -1)
            {
                _lineBuffer.Add((byte)_currByteLength);

                // Line found
                if ((_prevByte == (byte)'\r' && (byte)_currByteLength == (byte)'\n'))
                {
                    _result = new byte[_lineBuffer.Count - 2];    // Remove <CRLF>
                    _lineBuffer.CopyTo(0, _result, 0, _lineBuffer.Count - 2);

                    break;
                }

                // Store byte
                _prevByte = (byte)_currByteLength;

                // Read next byte
                _currByteLength = stream_source.ReadByte();
            }

            // Line isn't terminated with <CRLF> and has some chars left, return them.
            if (_lineBuffer.Count > 0 && _result == null)
            {
                _result = new byte[_lineBuffer.Count];
                _lineBuffer.CopyTo(0, _result, 0, _lineBuffer.Count);
            }

            return _result;
        }

        #endregion function StreamReadLine

        #region function ReadLine

        /// <summary>
        /// Reads line of data from Socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public string ReadLine(Socket socket)
        {
            return ReadLine(socket, 500, 60000);
        }

        /// <summary>
        /// Reads line of data from Socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="maxLen"></param>
        /// <param name="idleTimeOut"></param>
        /// <returns></returns>
        public string ReadLine(Socket socket, int maxLen, int idleTimeOut)
        {
            try
            {
                long lastDataTime = DateTime.Now.Ticks;
                ArrayList lineBuf = new ArrayList();
                byte prevByte = 0;

                while (true)
                {
                    if (socket.Available > 0)
                    {
                        // Read next byte
                        byte[] currByte = new byte[1];
                        var countRecieved = socket.Receive(currByte, 1, SocketFlags.None);
                        // Count must be equal. Eg. some computers won't give byte at first read attempt
                        if (countRecieved == 1)
                        {
                            lineBuf.Add(currByte[0]);

                            // Line found
                            if ((prevByte == (byte)'\r' && currByte[0] == (byte)'\n'))
                            {
                                byte[] retVal = new byte[lineBuf.Count - 2];    // Remove <CRLF>
                                lineBuf.CopyTo(0, retVal, 0, lineBuf.Count - 2);

                                return Encoding.UTF8.GetString(retVal).Trim();
                            }

                            // Store byte
                            prevByte = currByte[0];

                            // Check if maximum length is exceeded
                            if (lineBuf.Count > maxLen)
                            {
                                throw new ReadException(ReadReplyCode.LengthExceeded, "Maximum line length exceeded");
                            }

                            // reset last data time
                            lastDataTime = DateTime.Now.Ticks;
                        }
                    }
                    else
                    {
                        //---- Time out stuff -----------------------//
                        if (DateTime.Now.Ticks > lastDataTime + ((long)(idleTimeOut)) * 10000)
                        {
                            throw new ReadException(ReadReplyCode.TimeOut, "Read timeout");
                        }
                        System.Threading.Thread.Sleep(100);
                        //------------------------------------------//
                    }
                }
            }
            catch (Exception x)
            {
                if (x is ReadException)
                {
                    throw x;
                }
                throw new ReadException(ReadReplyCode.UnKnownError, x.Message);
            }
        }

        #endregion function ReadLine

        #region function SendLine

        /// <summary>
        /// Sends line to Socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="lineData"></param>
        public void SendLine(Socket socket, string lineData)
        {
            byte[] byte_data = Encoding.UTF8.GetBytes(lineData + "\r\n");
            var countSended = socket.Send(byte_data);
            if (countSended != byte_data.Length)
            {
                throw new Exception("Send error, didn't send all bytes !");
            }
        }

        #endregion function SendLine

        #region function ReadData

        /// <summary>
        /// Reads byte data from Socket while gets terminator or timeout.
        /// </summary>
        /// <param name="socket">Socket from to read data.</param>
        /// <param name="terminator">Terminator which terminates data.</param>
        /// <param name="removeFromEnd">Chars which will be removed from end of data.</param>
        /// <returns></returns>
        public byte[] ReadData(Socket socket, string terminator, string removeFromEnd)
        {
            MemoryStream _storeStream = new MemoryStream();

            var _replyCode = ReadData(socket, out _storeStream, null, 10000000, 300000, terminator, removeFromEnd);
            if (_replyCode != ReadReplyCode.Ok)
                throw new Exception("Error:" + _replyCode.ToString());

            return _storeStream.ToArray();
        }

        #endregion function ReadData

        #region function ReadData

        /// <summary>
        /// Reads specified count of data from Socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="storeStrm"></param>
        /// <param name="storeToStream">If true stores readed data to stream, otherwise just junks data.</param>
        /// <param name="cmdIdleTimeOut"></param>
        /// <returns></returns>
        public ReadReplyCode ReadData(Socket socket, long count, Stream storeStrm, bool storeToStream, int cmdIdleTimeOut)
        {
            var _replyCode = ReadReplyCode.Ok;

            try
            {
                long lastDataTime = DateTime.Now.Ticks;
                long readedCount = 0;
                while (readedCount < count)
                {
                    var countAvailable = socket.Available;
                    if (countAvailable > 0)
                    {
                        byte[] b = null;
                        if ((readedCount + countAvailable) <= count)
                        {
                            b = new byte[countAvailable];
                        }
                        // There are more data in socket than we need, just read as much we need
                        else
                        {
                            b = new byte[count - readedCount];
                        }

                        var countRecieved = socket.Receive(b, 0, b.Length, SocketFlags.None);
                        readedCount += countRecieved;

                        if (storeToStream && countRecieved > 0)
                        {
                            storeStrm.Write(b, 0, countRecieved);
                        }

                        // reset last data time
                        lastDataTime = DateTime.Now.Ticks;
                    }
                    else
                    {
                        //---- Idle and time out stuff ----------------------------------------//
                        if (DateTime.Now.Ticks > lastDataTime + ((long)(cmdIdleTimeOut)) * (long)10000)
                        {
                            _replyCode = ReadReplyCode.TimeOut;
                            break;
                        }
                        System.Threading.Thread.Sleep(50);
                        //---------------------------------------------------------------------//
                    }
                }
            }
            catch
            {
                _replyCode = ReadReplyCode.UnKnownError;
            }

            return _replyCode;
        }

        #endregion function ReadData

        #region function ReadData

        /// <summary>
        /// Reads reply from socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="replyData">Data that has been readen from socket.</param>
        /// <param name="addData">Data that has will be written at the beginning of read data. This param may be null.</param>
        /// <param name="maxLength">Maximum Length of data which may read.</param>
        /// <param name="cmdIdleTimeOut">Command idle time out in milliseconds.</param>
        /// <param name="terminator">Terminator string which terminates reading. eg '\r\n'.</param>
        /// <param name="removeFromEnd">Removes following string from reply.NOTE: removes only if ReadReplyCode is Ok.</param>
        /// <returns>Return reply code.</returns>
        public ReadReplyCode ReadData(Socket socket, out MemoryStream replyData, byte[] addData, int maxLength, int cmdIdleTimeOut, string terminator, string removeFromEnd)
        {
            var _replyCode = ReadReplyCode.Ok;

            replyData = new MemoryStream();

            try
            {
                FixedStack stack = new FixedStack(terminator);
                var nextReadWriteLen = 1;

                long lastDataTime = DateTime.Now.Ticks;
                while (nextReadWriteLen > 0)
                {
                    if (socket.Available >= nextReadWriteLen)
                    {
                        //Read byte(s)
                        byte[] b = new byte[nextReadWriteLen];
                        var countRecieved = socket.Receive(b);

                        // Write byte(s) to buffer, if length isn't exceeded.
                        if (_replyCode != ReadReplyCode.LengthExceeded)
                        {
                            replyData.Write(b, 0, countRecieved);
                        }

                        // Write to stack(terminator checker)
                        nextReadWriteLen = stack.Push(b, countRecieved);

                        //---- Check if maximum length is exceeded ---------------------------------//
                        if (_replyCode != ReadReplyCode.LengthExceeded && replyData.Length > maxLength)
                        {
                            _replyCode = ReadReplyCode.LengthExceeded;
                        }
                        //--------------------------------------------------------------------------//

                        // reset last data time
                        lastDataTime = DateTime.Now.Ticks;
                    }
                    else
                    {
                        //---- Idle and time out stuff ----------------------------------------//
                        if (DateTime.Now.Ticks > lastDataTime + ((long)(cmdIdleTimeOut)) * 10000)
                        {
                            _replyCode = ReadReplyCode.TimeOut;
                            break;
                        }
                        System.Threading.Thread.Sleep(50);
                        //---------------------------------------------------------------------//
                    }
                }

                // If reply is ok then remove chars if any specified by 'removeFromEnd'.
                if (_replyCode == ReadReplyCode.Ok && removeFromEnd.Length > 0)
                {
                    replyData.SetLength(replyData.Length - removeFromEnd.Length);
                }
            }
            catch
            {
                _replyCode = ReadReplyCode.UnKnownError;
            }

            return _replyCode;
        }

        #endregion function ReadData

        #region function SendData

        /// <summary>
        /// Sends data to socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="send_data">String data wich to send.</param>
        public void SendData(Socket socket, string send_data)
        {
            byte[] _dataBytes = Encoding.UTF8.GetBytes(send_data.ToCharArray());

            var _sentCount = socket.Send(_dataBytes, _dataBytes.Length, 0);
            if (_sentCount != _dataBytes.Length)
                throw new Exception("Smtp.SendData sended less data than requested !");
        }

        #endregion function SendData

        #region function ParseIP_from_EndPoint

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public string ParseIP_from_EndPoint(string endpoint)
        {
            var retVal = endpoint;

            var index = endpoint.IndexOf(":");
            if (index > 1)
            {
                retVal = endpoint.Substring(0, index);
            }

            return retVal;
        }

        #endregion function ParseIP_from_EndPoint

        #region function GetArgsText

        /// <summary>
        /// Gets argument part of command text.
        /// </summary>
        /// <param name="input">Input srting from where to remove value.</param>
        /// <param name="cmdTxtToRemove">Command text which to remove.</param>
        /// <returns></returns>
        public string GetArgsText(string input, string cmdTxtToRemove)
        {
            var buff = input.Trim();
            if (buff.Length >= cmdTxtToRemove.Length)
            {
                buff = buff.Substring(cmdTxtToRemove.Length);
            }
            buff = buff.Trim();

            return buff;
        }

        #endregion function GetArgsText

        #region function GetHostName

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address"></param>
        /// <returns></returns>
        public string GetHostName(string ip_address)
        {
            var _result = "UnkownHost";

            try
            {
                IPHostEntry _entry = System.Net.Dns.GetHostEntry(ip_address);
                _result = _entry.HostName;
            }
            catch
            {
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetHostName()
        {
            try
            {
                return System.Net.Dns.GetHostName();
            }
            catch
            {
                return "UnkownHost";
            }
        }

        #endregion function GetHostName

        #region function IsNumber

        /// <summary>
        /// Checks if specified string is number(long).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool IsNumber(string str)
        {
            long result = 0;
            return Int64.TryParse(str, out result);
        }

        #endregion function IsNumber

        #region function QDecode

        /// <summary>
        /// quoted-printable decoder.
        /// </summary>
        /// <param name="encoding">Input string encoding.</param>
        /// <param name="data">String which to encode.</param>
        /// <returns></returns>
        public string QDecode(Encoding encoding, string data)
        {
            MemoryStream _ostream = new MemoryStream();

            MemoryStream _istream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var _byte = _istream.ReadByte();

            while (_byte > -1)
            {
                // Hex eg. =E4
                if (_byte == '=')
                {
                    byte[] _buffer = new byte[2];
                    _istream.Read(_buffer, 0, 2);

                    // <CRLF> followed by =, it's splitted line
                    if (_buffer[0] != '\r' || _buffer[1] != '\n')
                    {
                        try
                        {
                            var _value = int.Parse(Encoding.UTF8.GetString(_buffer), System.Globalization.NumberStyles.HexNumber);
                            var _encodedChar = encoding.GetString(new byte[] { (byte)_value });

                            byte[] _data = Encoding.UTF8.GetBytes(_encodedChar);
                            _ostream.Write(_data, 0, _data.Length);
                        }
                        catch
                        {
                            // If worng hex value, just skip this chars
                        }
                    }
                }
                else
                {
                    var _encodedChar = encoding.GetString(new byte[] { (byte)_byte });

                    byte[] _data = Encoding.UTF8.GetBytes(_encodedChar);
                    _ostream.Write(_data, 0, _data.Length);
                }

                _byte = _istream.ReadByte();
            }

            return Encoding.UTF8.GetString(_ostream.ToArray());
        }

        #endregion function QDecode

        #region function IsAscii

        /// <summary>
        /// Checks if specified string data is acii data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsAscii(string data)
        {
            foreach (char c in data)
            {
                if ((int)c > 127)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion function IsAscii
    }
}