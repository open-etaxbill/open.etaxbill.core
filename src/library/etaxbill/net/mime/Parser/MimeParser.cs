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
using System.Collections;
using System.IO;
using System.Text;

namespace OdinSdk.eTaxBill.Net.Mime.Parser
{
    #region enum ContentDisposition

    /// <summary>
    /// Content disposition.
    /// </summary>
    public enum Disposition
    {
        /// <summary>
        /// Content is attachment.
        /// </summary>
        Attachment = 0,

        /// <summary>
        /// Content is embbed resource.
        /// </summary>
        Inline = 1,

        /// <summary>
        /// Content is unknown.
        /// </summary>
        Unknown = 40
    }

    #endregion enum ContentDisposition

    /// <summary>
    /// Mime parser.
    /// </summary>
    public class MimeParser
    {
        private string m_Headers = "";
        private string m_BoundaryID = "";
        private MemoryStream m_MsgStream = null;
        private ArrayList m_entries = null;

        private NetCore m_netCore = null;

        private NetCore Core
        {
            get
            {
                if (m_netCore == null)
                    m_netCore = new NetCore();

                return m_netCore;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="msg">Mime message which to parse.</param>
        public MimeParser(byte[] msg)
        {
            m_MsgStream = new MemoryStream(msg);

            m_Headers = ParseHeaders(m_MsgStream);
            m_BoundaryID = ParseBoundaryID(m_Headers);
        }

        #region function ParseHeaders

        /// <summary>
        /// Parses mime headers from message.
        /// </summary>
        /// <param name="msgStrm"></param>
        /// <returns></returns>
        private string ParseHeaders(MemoryStream msgStrm)
        {
            /*3.1.  GENERAL DESCRIPTION
            A message consists of header fields and, optionally, a body.
            The  body  is simply a sequence of lines containing ASCII charac-
            ters.  It is separated from the headers by a null line  (i.e.,  a
            line with nothing preceding the CRLF).
            */

            var headers = "";

            TextReader r = new StreamReader(msgStrm);
            var line = r.ReadLine();

            while (line != null && line.Length != 0)
            {
                headers += line + "\r\n";

                line = r.ReadLine();
            }

            headers += "\r\n";

            return headers;
        }

        #endregion function ParseHeaders

        #region function ParseFrom

        /// <summary>
        /// Parse sender from message.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private string ParseFrom(string headers)
        {
            using (TextReader r = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(headers))))
            {
                var line = r.ReadLine();

                while (line != null)
                {
                    if (line.ToUpper().StartsWith("FROM:"))
                    {
                        OdinSdk.eTaxBill.Net.Mime.Parser.eAddress e = new OdinSdk.eTaxBill.Net.Mime.Parser.eAddress(line.Substring(5).Trim());

                        return CDecode(e.Email);
                    }

                    line = r.ReadLine();
                }
            }

            return "";
        }

        #endregion function ParseFrom

        #region function ParseAddress

        private string[] ParseAddress(string headers, string fieldName)
        {
            var toFieldValue = ParseHeaderField(fieldName, headers);

            string[] tox = toFieldValue.Split(new char[] { ',' });
            for (int i = 0; i < tox.Length; i++)
            {
                tox[i] = CDecode(tox[i]);
            }

            return tox;
        }

        #endregion function ParseAddress

        #region function ParseSubject

        /// <summary>
        /// Parses subject from message.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private string ParseSubject(string headers)
        {
            var subjectFieldValue = ParseHeaderField("SUBJECT:", headers);

            return CDecode(subjectFieldValue);
        }

        #endregion function ParseSubject

        #region function ParseDate

        /// <summary>
        /// Parse message date.
        /// </summary>
        /// <param name="headers"></param>
        private DateTime ParseDate(string headers)
        {
            try
            {
                var date = ParseHeaderField("DATE:", headers);
                if (date.Length > 0)
                {
                    return ParseDateS(date);
                }
                else
                {
                    return DateTime.Today;
                }
            }
            catch
            {
                return DateTime.Today;
            }
        }

        #endregion function ParseDate

        #region function ParseMessageID

        /// <summary>
        /// Parse message ID.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private string ParseMessageID(string headers)
        {
            return ParseHeaderField("MESSAGE-ID:", headers);
        }

        #endregion function ParseMessageID

        #region function ParseContentType

        /// <summary>
        /// Parse content type.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private string ParseContentType(string headers)
        {
            //	return ParseHeaderField("CONTENT-TYPE:",headers);

            using (TextReader r = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(headers))))
            {
                var line = r.ReadLine();

                while (line != null)
                {
                    if (line.ToUpper().StartsWith("CONTENT-TYPE:"))
                    {
                        return line.Substring(13).Trim();
                    }

                    line = r.ReadLine();
                }
            }

            return "";
        }

        #endregion function ParseContentType

        #region function ParseBoundaryID

        /// <summary>
        /// Parse boundaryID.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        internal string ParseBoundaryID(string headers)
        {
            using (TextReader r = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(headers))))
            {
                var line = r.ReadLine();

                while (line != null)
                {
                    var index = line.ToUpper().IndexOf("BOUNDARY=");
                    if (index > -1)
                    {
                        line = line.Substring(index + 9); // Remove charset=

                        // Charset may be in "" and without
                        if (line.StartsWith("\""))
                        {
                            return line.Substring(1, line.IndexOf("\"", 1) - 1);
                        }
                        else
                        {
                            var endIndex = line.Length;
                            if (line.IndexOf(" ") > -1)
                            {
                                endIndex = line.IndexOf(" ");
                            }

                            return line.Substring(0, endIndex);
                        }
                    }

                    line = r.ReadLine();

                    /*	int index = line.ToUpper().IndexOf("BOUNDARY=");
                        if(index > -1){
                            line = line.Substring(index + 10); // Remove BOUNDARY="
                            return line.Substring(0,line.IndexOf("\""));
                        }

                        line = r.ReadLine();*/
                }
            }

            return "";
        }

        #endregion function ParseBoundaryID

        #region function ParseEntries

        /// <summary>
        /// Parses mime entries.
        /// </summary>
        /// <param name="_messageStream"></param>
        /// <param name="pos"></param>
        /// <param name="boundaryID"></param>
        internal ArrayList ParseEntries(MemoryStream _messageStream, int pos, string boundaryID)
        {
            ArrayList _entries = m_entries;

            // Entries are already parsed
            if (m_entries == null && _messageStream != null)
            {
                _entries = new ArrayList();

                // If message doesn't have entries (simple text message).
                if (this.ContentType.ToLower().IndexOf("text/") > -1)
                {
                    _entries.Add(new MimeEntry(_messageStream.ToArray(), this));
                    m_entries = _entries;

                    return m_entries;
                }

                _messageStream.Position = pos;

                if (boundaryID.Length > 0)
                {
                    MemoryStream _entryStream = new MemoryStream();
                    byte[] _lineData = this.Core.StreamReadLine(_messageStream);

                    // Search first entry
                    while (_lineData != null)
                    {
                        var line = Encoding.UTF8.GetString(_lineData);
                        if (line.StartsWith("--" + boundaryID))
                        {
                            break;
                        }

                        _lineData = this.Core.StreamReadLine(_messageStream);
                    }

                    // Start reading entries
                    while (_lineData != null)
                    {
                        // Read entry data
                        var line = Encoding.UTF8.GetString(_lineData);
                        // Next boundary
                        if (line.StartsWith("--" + boundaryID) && _entryStream.Length > 0)
                        {
                            // Add Entry
                            _entries.Add(new MimeEntry(_entryStream.ToArray(), this));

                            _entryStream.SetLength(0);
                        }
                        else
                        {
                            _entryStream.Write(_lineData, 0, _lineData.Length);
                            _entryStream.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
                        }

                        _lineData = this.Core.StreamReadLine(_messageStream);
                    }
                }
            }

            return _entries;
        }

        #endregion function ParseEntries

        #region function CDecode

        private string CDecode(string data)
        {
            if (data.IndexOf("=?") > -1)
            {
                var _index = data.IndexOf("=?");

                string[] _parts = data.Substring(_index + 2).Split(new char[] { '?' });

                var _name = _parts[0];
                var _type = _parts[1];
                var _datax = _parts[2];

                Encoding _encoding = Encoding.GetEncoding(_name);
                if (_type.ToUpper() == "Q")
                {
                    return this.Core.QDecode(_encoding, _datax);
                }

                if (_type.ToUpper() == "B")
                {
                    return _encoding.GetString(Convert.FromBase64String(_datax));
                }
            }

            return data;
        }

        #endregion function CDecode

        #region static function ParseDate

        /// <summary>
        /// Parses rfc2822 datetime.
        /// </summary>
        /// <param name="date_string">Date string</param>
        /// <returns></returns>
        public static DateTime ParseDateS(string date_string)
        {
            /*
            GMT  -0000
            EDT  -0400
            EST  -0500
            CDT  -0500
            CST  -0600
            MDT  -0600
            MST  -0700
            PDT  -0700
            PST  -0800
            */

            date_string = date_string.Replace("GMT", "-0000");
            date_string = date_string.Replace("EDT", "-0400");
            date_string = date_string.Replace("EST", "-0500");
            date_string = date_string.Replace("CDT", "-0500");
            date_string = date_string.Replace("CST", "-0600");
            date_string = date_string.Replace("MDT", "-0600");
            date_string = date_string.Replace("MST", "-0700");
            date_string = date_string.Replace("PDT", "-0700");
            date_string = date_string.Replace("PST", "-0800");

            string[] formats = new string[]{
                "r",
                "ddd, d MMM yyyy HH':'mm':'ss zzz",
                "ddd, dd MMM yyyy HH':'mm':'ss zzz",
                "dd'-'MMM'-'yyyy HH':'mm':'ss zzz",
                "d'-'MMM'-'yyyy HH':'mm':'ss zzz"
            };

            return DateTime.ParseExact(date_string.Trim(), formats, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
        }

        #endregion static function ParseDate

        #region static function ParseHeaderField

        /// <summary>
        /// Parse header specified header field.
        /// </summary>
        /// <param name="fieldName">Header field which to parse.</param>
        /// <param name="headers">Full headers.</param>
        public static string ParseHeaderField(string fieldName, string headers)
        {
            using (TextReader r = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(headers))))
            {
                var line = r.ReadLine();

                while (line != null)
                {
                    // Find line where field begins
                    if (line.ToUpper().StartsWith(fieldName.ToUpper()))
                    {
                        // Remove field name and start reading value
                        var fieldValue = line.Substring(fieldName.Length).Trim();

                        // see if multi line value. NOTE: multi line value starts with <TAB> beginning of line.
                        line = r.ReadLine();
                        while (line.StartsWith("\t"))
                        {
                            fieldValue += line.Trim();
                            line = r.ReadLine();
                        }

                        return fieldValue;
                    }

                    line = r.ReadLine();
                }
            }

            return "";
        }

        #endregion static function ParseHeaderField

        #region Properties Implementation

        /// <summary>
        /// Gets message headers.
        /// </summary>
        public string Headers
        {
            get
            {
                return m_Headers;
            }
        }

        /// <summary>
        /// Gets sender.
        /// </summary>
        public string From
        {
            get
            {
                return ParseFrom(m_Headers);
            }
        }

        /// <summary>
        /// Gets recipients.
        /// </summary>
        public string[] To
        {
            get
            {
                return ParseAddress(m_Headers, "TO:");
            }
        }

        /// <summary>
        /// Gets cc.
        /// </summary>
        public string[] Cc
        {
            get
            {
                return ParseAddress(m_Headers, "CC:");
            }
        }

        /// <summary>
        /// Gets bcc.
        /// </summary>
        public string[] Bcc
        {
            get
            {
                return ParseAddress(m_Headers, "BCC:");
            }
        }

        /// <summary>
        /// Gets subject.
        /// </summary>
        public string Subject
        {
            get
            {
                return ParseSubject(m_Headers);
            }
        }

        /// <summary>
        /// Gets message body text.
        /// </summary>
        public string BodyText
        {
            get
            {
                m_entries = ParseEntries(m_MsgStream, m_Headers.Length, m_BoundaryID);

                // Find first text entry
                foreach (MimeEntry ent in m_entries)
                {
                    if (ent.MimeEntries != null)
                    {
                        foreach (MimeEntry ent1 in ent.MimeEntries)
                        {
                            if (ent1.MimeEntries != null)
                            {
                                foreach (MimeEntry ent2 in ent1.MimeEntries)
                                {
                                    if (ent2.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1)
                                    {
                                        return ent2.DataS;
                                    }
                                }
                            }

                            if (ent1.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1)
                            {
                                return ent1.DataS;
                            }
                        }
                    }

                    if (ent.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1)
                    {
                        return ent.DataS;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// Gets messageID.
        /// </summary>
        public string MessageID
        {
            get
            {
                return ParseMessageID(m_Headers);
            }
        }

        /// <summary>
        /// Gets messageID.
        /// </summary>
        public string ContentType
        {
            get
            {
                return ParseContentType(m_Headers);
            }
        }

        /// <summary>
        /// Gets message date.
        /// </summary>
        public DateTime MessageDate
        {
            get
            {
                return ParseDate(m_Headers);
            }
        }

        /// <summary>
        /// Gets mime entries.
        /// </summary>
        public ArrayList MimeEntries
        {
            get
            {
                m_entries = ParseEntries(m_MsgStream, m_Headers.Length, m_BoundaryID);

                return m_entries;
            }
        }

        #endregion Properties Implementation
    }
}