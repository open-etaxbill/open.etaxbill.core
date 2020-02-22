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

namespace OdinSdk.eTaxBill.Net.Dns
{
    /// <summary>
    /// Summary description for Dns_Answer.
    /// </summary>
    internal class Dns_Answer
    {
        private string m_NAME = "";
        private QTYPE m_QTYPE = QTYPE.MX;
        private int m_CLASS = 1;
        private int m_TTL = 0;
        private int m_RDLENGTH = 0;
        private object m_AnswerObj = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dns_Answer(string name, QTYPE qType, int rdClass, int ttl, int rdLength, object answerObj)
        {
            m_NAME = name;
            m_QTYPE = qType;
            m_CLASS = rdClass;
            m_TTL = ttl;
            m_RDLENGTH = rdLength;
            m_AnswerObj = answerObj;
        }

        #region function GetRecordType

        /// <summary>
        /// Gets query type eg. MX.
        /// </summary>
        /// <returns></returns>
        public QTYPE GetRecordType()
        {
            if (m_AnswerObj is MX_Record)
            {
                return QTYPE.MX;
            }

            return QTYPE.UnKnown;
        }

        #endregion function GetRecordType

        #region Properties Implementation

        /// <summary>
        /// A domain name to which this resource record pertains.
        /// </summary>
        public string NAME
        {
            get
            {
                return m_NAME;
            }
        }

        /// <summary>
        /// This field specifies the meaning of the data in the RDATA field.
        /// </summary>
        public QTYPE QTYPE
        {
            get
            {
                return m_QTYPE;
            }
        }

        /// <summary>
        /// Two octets which specify the class of the data in the RDATA field.
        /// </summary>
        public int CLASS
        {
            get
            {
                return m_CLASS;
            }
        }

        /// <summary>
        /// a 32 bit unsigned integer that specifies the time
        /// interval (in seconds) that the resource record may be
        /// cached before it should be discarded.
        /// </summary>
        public int TTL
        {
            get
            {
                return m_TTL;
            }
        }

        /// <summary>
        /// An unsigned 16 bit integer that specifies the length in octets of the RDATA field.
        /// </summary>
        public int RDLENGTH
        {
            get
            {
                return m_RDLENGTH;
            }
        }

        /// <summary>
        /// Gets answer object.
        /// </summary>
        public object RecordObj
        {
            get
            {
                return m_AnswerObj;
            }
        }

        #endregion Properties Implementation
    }
}