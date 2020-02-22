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
    /// Summary description for Dns_Header.
    /// </summary>
    internal class Dns_Header
    {
        private int m_QID = 0;
        private OPCODE m_OPCODE = OPCODE.QUERY;
        private RCODE m_RCODE = RCODE.NO_ERROR;
        private int m_QDCOUNT = 0;
        private int m_ANCOUNT = 0;
        private int m_NSCOUNT = 0;
        private int m_ARCOUNT = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dns_Header()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="opcode"></param>
        public Dns_Header(int id, OPCODE opcode)
        {
            m_QID = id;
            m_OPCODE = opcode;
            m_QDCOUNT = 1;
        }

        #region function GetHeader

        /// <summary>
        /// Gets header.
        /// </summary>
        /// <returns></returns>
        public byte[] GetHeader()
        {
            byte[] header = null;

            try
            {
                /* 4.1.1. Header section format
                                              1  1  1  1  1  1
                0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                      ID                       |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                    QDCOUNT                    |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                    ANCOUNT                    |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                    NSCOUNT                    |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                    ARCOUNT                    |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                */

                header = new byte[12];
                //--------- Header part -----------------------------------//
                header[0] = (byte)(m_QID >> 8);
                header[1] = (byte)m_QID;
                header[2] = (byte)1;
                header[3] = (byte)0;
                header[4] = (byte)0;
                header[5] = (byte)m_QDCOUNT;
                header[6] = (byte)0;
                header[7] = (byte)0;
                header[8] = (byte)0;
                header[9] = (byte)0;
                header[10] = (byte)0;
                header[11] = (byte)0;
                //---------------------------------------------------------//
            }
            catch
            {
            }

            return header;
        }

        #endregion function GetHeader

        #region function ParseHeader

        internal bool ParseHeader(byte[] query)
        {
            try
            {
                /*
                                                1  1  1  1  1  1
                  0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |                      ID                       |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |                    QDCOUNT                    |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |                    ANCOUNT                    |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |                    NSCOUNT                    |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                 |                    ARCOUNT                    |
                 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

                QDCOUNT
                    an unsigned 16 bit integer specifying the number of
                    entries in the question section.

                ANCOUNT
                    an unsigned 16 bit integer specifying the number of
                    resource records in the answer section.
                */

                //----- Parse query headers -----------------------------//
                // Get reply code
                var id = (query[0] << 8 | query[1]);
                var opcode = ((query[2] >> 3) & 15);
                var replyCode = (query[3] & 15);
                var queryCount = (query[4] << 8 | query[5]);
                var answerCount = (query[6] << 8 | query[7]);
                var nsCount = (query[8] << 8 | query[9]);
                var arCount = (query[10] << 8 | query[11]);
                //-------------------------------------------------------//

                m_QID = id;
                m_OPCODE = (OPCODE)opcode;
                m_RCODE = (RCODE)replyCode;
                m_QDCOUNT = queryCount;
                m_ANCOUNT = answerCount;
                m_NSCOUNT = nsCount;
                m_ARCOUNT = arCount;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion function ParseHeader

        #region Properties Implementation

        /// <summary>
        /// A 16 bit identifier assigned by the program that
        /// generates any kind of query.  This identifier is copied
        /// the corresponding reply and can be used by the requester
        /// to match up replies to outstanding queries.
        /// </summary>
        public int ID
        {
            get
            {
                return m_QID;
            }
        }

        /// <summary>
        /// A four bit field that specifies kind of query in this
        /// message.  This value is set by the originator of a query
        /// and copied into the response.
        /// </summary>
        public OPCODE OPCODE
        {
            get
            {
                return m_OPCODE;
            }
        }

        /// <summary>
        /// Response code - this 4 bit field is set as part of responses.
        /// </summary>
        public RCODE RCODE
        {
            get
            {
                return m_RCODE;
            }
        }

        /// <summary>
        /// an unsigned 16 bit integer specifying the number of
        /// entries in the question section.
        /// </summary>
        public int QDCOUNT
        {
            get
            {
                return m_QDCOUNT;
            }
        }

        /// <summary>
        /// an unsigned 16 bit integer specifying the number of
        /// resource records in the answer section.
        /// </summary>
        public int ANCOUNT
        {
            get
            {
                return m_ANCOUNT;
            }
        }

        /// <summary>
        /// an unsigned 16 bit integer specifying the number of name
        /// server resource records in the authority records section.
        /// </summary>
        public int NSCOUNT
        {
            get
            {
                return m_NSCOUNT;
            }
        }

        /// <summary>
        /// an unsigned 16 bit integer specifying the number of
        /// resource records in the additional records section.
        /// </summary>
        public int ARCOUNT
        {
            get
            {
                return m_ARCOUNT;
            }
        }

        #endregion Properties Implementation
    }
}