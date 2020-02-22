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
    #region enum OPCODE

    /// <summary>
    ///
    /// </summary>
    internal enum OPCODE
    {
        /// <summary>
        ///  a standard query.
        /// </summary>
        QUERY = 0,

        /// <summary>
        /// an inverse query.
        /// </summary>
        IQUERY = 1,

        /// <summary>
        /// a server status request.
        /// </summary>
        STATUS = 2,
    }

    #endregion enum OPCODE

    #region enum QTYPE

    /// <summary>
    /// 큂uery type.
    /// </summary>
    internal enum QTYPE
    {
        /// <summary>
        /// a host address
        /// </summary>
        A = 1,

        /// <summary>
        /// an authoritative name server
        /// </summary>
        NS = 2,

        //	MD    = 3,  Obsolete
        //	MF    = 5,  Obsolete

        /// <summary>
        /// the canonical name for an alias
        /// </summary>
        CNAME = 5,

        /// <summary>
        /// marks the start of a zone of authority
        /// </summary>
        SOA = 6,

        //	MB    = 7,  EXPERIMENTAL
        //	MG    = 8,  EXPERIMENTAL
        //  MR    = 9,  EXPERIMENTAL
        //	NULL  = 10, EXPERIMENTAL
        /// <summary>
        /// a well known service description
        /// </summary>
        WKS = 11,

        /// <summary>
        /// a domain name pointer
        /// </summary>
        PTR = 12,

        /// <summary>
        /// host information
        /// </summary>
        HINFO = 13,

        /// <summary>
        /// mailbox or mail list information
        /// </summary>
        MINFO = 14,

        /// <summary>
        /// mail exchange
        /// </summary>
        MX = 15,

        /// <summary>
        /// text strings
        /// </summary>
        TXT = 16,

        /// <summary>
        /// UnKnown
        /// </summary>
        UnKnown = 9999,
    }

    #endregion enum QTYPE

    #region enum RCODE

    /// <summary>
    /// Dns server reply codes.
    /// </summary>
    internal enum RCODE
    {
        /// <summary>
        /// No error condition.
        /// </summary>
        NO_ERROR = 0,

        /// <summary>
        /// Format error - The name server was unable to interpret the query.
        /// </summary>
        FORMAT_ERRROR = 1,

        /// <summary>
        /// Server failure - The name server was unable to process this query due to a problem with the name server.
        /// </summary>
        SERVER_FAILURE = 2,

        /// <summary>
        /// Name Error - Meaningful only for responses from an authoritative name server, this code signifies that the
        /// domain name referenced in the query does not exist.
        /// </summary>
        NAME_ERROR = 3,

        /// <summary>
        /// Not Implemented - The name server does not support the requested kind of query.
        /// </summary>
        NOT_IMPLEMENTED = 4,

        /// <summary>
        /// Refused - The name server refuses to perform the specified operation for policy reasons.
        /// </summary>
        REFUSED = 5,
    }

    #endregion enum RCODE
}