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

using System.Collections;
using System.Text;

namespace OdinSdk.eTaxBill.Net.Dns
{
    /// <summary>
    /// This class holds Dns answers returned by server.
    /// </summary>
    internal class Dns_Answers
    {
        private Dns_Answer[] m_Answers = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dns_Answers()
        {
        }

        #region function ParseAnswers

        /// <summary>
        /// Parses answer.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="queryID"></param>
        /// <returns>Returns true if answer parsed successfully.</returns>
        internal bool ParseAnswers(byte[] reply, int queryID)
        {
            try
            {
                /*
                                               1  1  1  1  1  1
                 0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                                               |
                /                                               /
                /                      NAME                     /
                |                                               |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                      TYPE                     |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                     CLASS                     |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                      TTL                      |
                |                                               |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                   RDLENGTH                    |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
                /                     RDATA                     /
                /                                               /
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                */

                //------- Parse result -----------------------------------//

                Dns_Header replyHeader = new Dns_Header();
                if (replyHeader.ParseHeader(reply) == false)
                    return false;

                // Check that it's query what we want
                if (queryID != replyHeader.ID)
                    return false;

                var pos = 12;

                //----- Parse question part ------------//
                for (int q = 0; q < replyHeader.QDCOUNT; q++)
                {
                    var dummy = "";
                    GetQName(reply, ref pos, ref dummy);
                    //qtype + qclass
                    pos += 4;
                }
                //--------------------------------------//

                ArrayList answers = new ArrayList();

                //---- Start parsing aswers ------------------------------------------------------------------//
                for (int i = 0; i < replyHeader.ANCOUNT; i++)
                {
                    var name = "";
                    if (GetQName(reply, ref pos, ref name) == false)
                        return false;

                    var type = reply[pos++] << 8 | reply[pos++];
                    var rdClass = reply[pos++] << 8 | reply[pos++];
                    var ttl = reply[pos++] << 24 | reply[pos++] << 16 | reply[pos++] << 8 | reply[pos++];
                    var rdLength = reply[pos++] << 8 | reply[pos++];

                    object answerObj = null;
                    switch ((QTYPE)type)
                    {
                        case QTYPE.MX:
                            answerObj = ParseMxRecord(reply, ref pos);
                            break;

                        default:
                            answerObj = "sgas"; // Dummy place holder for now
                            pos += rdLength;
                            break;
                    }

                    // Add answer to answer list
                    if (answerObj != null)
                    {
                        answers.Add(new Dns_Answer(name, (QTYPE)type, rdClass, ttl, rdLength, answerObj));
                    }
                    else
                    {
                        return false; // Parse failed
                    }
                }
                //-------------------------------------------------------------------------------------------//

                if (answers.Count > 0)
                {
                    m_Answers = new Dns_Answer[answers.Count];
                    answers.CopyTo(m_Answers);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion function ParseAnswers

        #region function GetQName

        private bool GetQName(byte[] reply, ref int offset, ref string name)
        {
            try
            {
                // Do while not terminator
                while (reply[offset] != 0)
                {
                    // Check if it's pointer(In pointer first two bits always 1)
                    var isPointer = ((reply[offset] & 0xC0) == 0xC0);

                    // If pointer
                    if (isPointer)
                    {
                        var pStart = ((reply[offset] & 0x3F) << 8) | (reply[++offset]);
                        offset++;
                        return GetQName(reply, ref pStart, ref name);
                    }
                    else
                    {
                        // label lenght (length = 8Bit and first 2 bits always 0)
                        var labelLenght = (reply[offset] & 0x3F);
                        offset++;

                        // Copy label into name
                        name += Encoding.UTF8.GetString(reply, offset, labelLenght);
                        offset += labelLenght;
                    }

                    // If the next char isn't terminator,
                    // label continues - add dot between two labels
                    if (reply[offset] != 0)
                    {
                        name += ".";
                    }
                }

                // Move offset by terminator lenght
                offset++;

                return true;
            }
            catch//(Exception x)
            {
                //		System.Windows.Forms.MessageBox.Show(x.Message);
                return false;
            }
        }

        #endregion function GetQName

        #region function ParseMxRecord

        /// <summary>
        /// Parses MX record.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="offset"></param>
        /// <returns>Returns null, if failed.</returns>
        private object ParseMxRecord(byte[] reply, ref int offset)
        {
            /* RFC 1035	3.3.9. MX RDATA format

            +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            |                  PREFERENCE                   |
            +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            /                   EXCHANGE                    /
            /                                               /
            +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

            where:

            PREFERENCE
                A 16 bit integer which specifies the preference given to
                this RR among others at the same owner.  Lower values
                are preferred.

            EXCHANGE
                A <domain-name> which specifies a host willing to act as
                a mail exchange for the owner name.
            */

            try
            {
                var pref = reply[offset++] << 8 | reply[offset++];

                var name = "";
                if (GetQName(reply, ref offset, ref name))
                {
                    return new MX_Record(pref, name);
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion function ParseMxRecord

        #region function GetMxRecordsFromAnswers

        /// <summary>
        /// Gets MX records from answer collection and ORDERS them by preference.
        /// NOTE: Duplicate preference records are appended to end.
        /// </summary>
        /// <returns></returns>
        internal MX_Record[] GetMxRecordsFromAnswers()
        {
            MX_Record[] _result = null;

            try
            {
                SortedList mx = new SortedList();

                ArrayList duplicateList = new ArrayList();
                foreach (Dns_Answer answer in m_Answers)
                {
                    if (answer.QTYPE == QTYPE.MX)
                    {
                        MX_Record mxRec = (MX_Record)answer.RecordObj;

                        if (mx.Contains(mxRec.Preference) == false)
                        {
                            mx.Add(mxRec.Preference, mxRec);
                        }
                        else
                        {
                            duplicateList.Add(mxRec);
                        }
                    }
                }

                MX_Record[] mxBuff = new MX_Record[mx.Count + duplicateList.Count];
                mx.Values.CopyTo(mxBuff, 0);
                duplicateList.CopyTo(mxBuff, mx.Count);
                _result = mxBuff;
            }
            catch
            {
            }

            return _result;
        }

        #endregion function GetMxRecordsFromAnswers

        #region Properties Implementation

        /// <summary>
        /// Gets answers.
        /// </summary>
        public Dns_Answer[] Answers
        {
            get
            {
                return m_Answers;
            }
        }

        #endregion Properties Implementation
    }
}