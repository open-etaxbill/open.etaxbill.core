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
using System.Net;
using System.Net.Sockets;

namespace OdinSdk.eTaxBill.Net.Dns
{
    /// <summary>
    /// Dns reply codes.
    /// </summary>
    public enum DnsReplyCode
    {
        /// <summary>
        /// Requested records retrieved sucessfully.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// No requested records found.
        /// </summary>
        NoEntries = 1,

        /// <summary>
        /// There was error retrieving records.
        /// </summary>
        TempError = 2,
    }

    /// <summary>
    /// Dns.
    /// </summary>
    public class DnsEx
    {
        private static string[] m_DnsServers = null;
        private static bool m_UseDnsCache = true;
        private static int m_ID = 100;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DnsEx()
        {
            if (DnsCache.CacheInited == false)
                DnsCache.InitNewCache();
        }

        #region function GetMXRecords

        /// <summary>
        /// Gets MX records.(MX records are sorted by preference, lower array element is prefered)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="mxRecords"></param>
        /// <returns></returns>
        public DnsReplyCode GetMXRecords(string domain, out MX_Record[] mxRecords)
        {
            mxRecords = new MX_Record[0];

            //--- Try to get MX records from cache
            if (m_UseDnsCache)
            {
                mxRecords = DnsCache.GetMXFromCache(domain);
                if (mxRecords != null)
                {
                    //		Console.WriteLine("domain:" + domain + " from cahce.");
                    return DnsReplyCode.Ok;
                }
            }
            //------------------------------------//

            Dns_Header header = new Dns_Header(DnsEx.ID, OPCODE.IQUERY);
            Dns_Query query = new Dns_Query(domain, QTYPE.MX, 1);

            byte[] bQuery = query.GetQuery(header);

            byte[] reply = GetQuery(bQuery, header.ID);
            if (reply != null)
            {
                Dns_Answers answers = new Dns_Answers();
                if (answers.ParseAnswers(reply, header.ID))
                {
                    mxRecords = answers.GetMxRecordsFromAnswers();

                    if (mxRecords != null)
                    {
                        // Add to cache
                        DnsCache.AddMXToCache(domain, mxRecords);
                        return DnsReplyCode.Ok;
                    }
                    else
                    {
                        return DnsReplyCode.NoEntries;
                    }
                }
            }

            return DnsReplyCode.TempError;
        }

        #endregion function GetMXRecords

        #region function GetQuery

        private byte[] GetQuery(byte[] query, int queryID)
        {
            byte[] retVal = null;

            try
            {
                var helper = 0;
                var count = 0;
                while (count < 10)
                {
                    if (count > m_DnsServers.Length)
                    {
                        helper = 0;
                    }

                    byte[] reply = QueryServer(query, m_DnsServers[helper]);
                    Dns_Answers answers = new Dns_Answers();

                    // If reply is ok, return it
                    if (reply != null && answers.ParseAnswers(reply, queryID))
                    {
                        return reply;
                    }

                    count++;
                    helper++;
                }
            }
            catch
            {
            }

            return retVal;
        }

        #endregion function GetQuery

        #region function QueryServer

        private byte[] QueryServer(byte[] query, string server_ip)
        {
            var _result = new byte[0];

            try
            {
                var _remote_endpoint = new IPEndPoint(IPAddress.Parse(server_ip), 53);
                var _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                var _local_endpoint = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
                _client.Bind(_local_endpoint);

                _client.Connect(_remote_endpoint);

                //send query
                _client.Send(query);

                // Wait until we have a reply
                if (_client.Poll(5 * 1000000, SelectMode.SelectRead))
                {
                    _result = new byte[512];
                    _client.Receive(_result);
                }

                _client.Close();
            }
            catch (Exception)
            {
            }

            return _result;
        }

        #endregion function QueryServer

        #region Properties Implementation

        /// <summary>
        /// Gets or sets dns servers.
        /// </summary>
        public static string[] DnsServers
        {
            get
            {
                return m_DnsServers;
            }

            set
            {
                m_DnsServers = value;
            }
        }

        /// <summary>
        /// Gets or sets if to use dns caching.
        /// </summary>
        public static bool UseDnsCache
        {
            get
            {
                return m_UseDnsCache;
            }

            set
            {
                m_UseDnsCache = value;
            }
        }

        internal static int ID
        {
            get
            {
                if (m_ID >= 65535)
                {
                    m_ID = 100;
                }
                return m_ID++;
            }
        }

        #endregion Properties Implementation
    }
}