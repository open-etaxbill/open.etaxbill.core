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

using OdinSdk.OdinLib.Configuration;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/queue/QAgency/2015/12")]
    [Serializable]
    public class QAgency : ICloneable, IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        [DataMember(Name = "Certkey", Order = 0)]
        private Guid m_certkey = Guid.Empty;

        [DataMember(Name = "QueueName", Order = 1)]
        private string m_queue_name = "";

        [DataMember(Name = "HostName", Order = 2)]
        private string m_host_name = "";

        [DataMember(Name = "IpAddress", Order = 3)]
        private string m_ip_address = "";

        [DataMember(Name = "Protocol", Order = 4)]
        private string m_protocol = "";

        [DataMember(Name = "CategoryId", Order = 5)]
        private string m_category_id = "";

        [DataMember(Name = "product_name", Order = 6)]
        private string m_productName = "";

        [DataMember(Name = "ProductId", Order = 7)]
        private string m_product_id = "";

        [DataMember(Name = "pVersion", Order = 8)]
        private string m_product_version = "";

        [DataMember(Name = "UsingL4", Order = 9)]
        private string m_usingL4 = "";

        [DataMember(Name = "IpAddressL4", Order = 10)]
        private string m_ipAddressL4 = "";

        [DataMember(Name = "Connected", Order = 11)]
        private DateTime m_connected = CUnixTime.UtcNow;

        [DataMember(Name = "UserName", Order = 12)]
        private string m_userName = "";

        [DataMember(Name = "UserId", Order = 13)]
        private string m_user_id = "";

        [DataMember(Name = "PingRetry", Order = 14)]
        private int m_pingRetry = 0;

        [DataMember(Name = "IsService", Order = 15)]
        private bool m_isService = true;

        [DataMember(Name = "IssuedHost", Order = 16)]
        private string m_issuedHost;

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public QAgency()
            : this("", "")
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        public QAgency(string protocol, string category_id)
        {
            Certkey = Guid.NewGuid();

            Protocol = String.IsNullOrEmpty(protocol) ? "tcp" : protocol;
            CategoryId = category_id;

            IssuedHost = HostName = CfgHelper.SNG.MachineName;
            IpAddress = CfgHelper.SNG.IPAddress;

            Connected = CUnixTime.UtcNow;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version)
            : this(protocol, category_id)
        {
            this.ProductName = product_name;
            this.ProductId = product_id;
            this.ProductVersion = product_version;

            this.QueueName = product_name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="is_service"></param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version, bool is_service)
            : this(protocol, category_id, product_name, product_id, product_version)
        {
            this.IsService = is_service;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="queue_name"></param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version, string queue_name)
            : this(protocol, category_id, product_name, product_id, product_version)
        {
            this.QueueName = queue_name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="user_name"></param>
        /// <param name="user_id"></param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id)
            : this(protocol, category_id, product_name, product_id, product_version)
        {
            this.UserName = user_name;
            this.UserId = user_id;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="user_name"></param>
        /// <param name="user_id"></param>
        /// <param name="queue_name"></param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id, string queue_name)
            : this(protocol, category_id, product_name, product_id, product_version, user_name, user_id)
        {
            this.QueueName = queue_name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="usingL4"></param>
        /// <param name="ip_addressL4"></param>
        /// <param name="user_name"></param>
        /// <param name="user_id"></param>
        /// <param name="queue_name"></param>
        public QAgency(string protocol, string category_id, string product_name, string product_id, string product_version, string usingL4, string ip_addressL4, string user_name, string user_id, string queue_name)
            : this(protocol, category_id, product_name, product_id, product_version, user_name, user_id)
        {
            this.UsingL4 = usingL4;
            this.IpAddressL4 = ip_addressL4;
            this.QueueName = queue_name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public Guid Certkey
        {
            get
            {
                return m_certkey;
            }
            set
            {
                m_certkey = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string IpAddress
        {
            get
            {
                return m_ip_address;
            }
            set
            {
                m_ip_address = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string HostName
        {
            get
            {
                return m_host_name;
            }
            set
            {
                m_host_name = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string Protocol
        {
            get
            {
                return m_protocol;
            }
            set
            {
                m_protocol = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string CategoryId
        {
            get
            {
                return m_category_id;
            }
            set
            {
                m_category_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ProductName
        {
            get
            {
                return m_productName;
            }
            set
            {
                m_productName = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ProductId
        {
            get
            {
                return m_product_id;
            }
            set
            {
                m_product_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ProductVersion
        {
            get
            {
                return m_product_version;
            }
            set
            {
                m_product_version = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string UsingL4
        {
            get
            {
                return m_usingL4;
            }
            set
            {
                m_usingL4 = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string IpAddressL4
        {
            get
            {
                return m_ipAddressL4;
            }
            set
            {
                m_ipAddressL4 = value;
            }
        }

        /// <summary>
        /// 큐 명칭
        /// </summary>
        public string QueueName
        {
            get
            {
                return m_queue_name;
            }
            set
            {
                m_queue_name = value;
            }
        }

        /// <summary>
        /// 연결 되어진 일시
        /// </summary>
        public DateTime Connected
        {
            get
            {
                return m_connected;
            }
            set
            {
                m_connected = value;
            }
        }

        /// <summary>
        /// 사용자 이름
        /// </summary>
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// 사용자 로그인 ID
        /// </summary>
        public string UserId
        {
            get
            {
                return m_user_id;
            }
            set
            {
                m_user_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int PingRetry
        {
            get
            {
                return m_pingRetry;
            }
            set
            {
                m_pingRetry = value;
            }
        }

        /// <summary>
        /// 다른 서비스를 이용하는 서비스인 경우 true
        /// TopUrl, Logger등과 같은 서비스는 다른 서비스를 이용하지 않습니다.
        /// 이런경우 false로 해 놓으면 로그가 로컬에 파일로 저장 됩니다.
        /// </summary>
        public bool IsService
        {
            get
            {
                return m_isService;
            }
            set
            {
                m_isService = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string IssuedHost
        {
            get
            {
                return m_issuedHost;
            }
            set
            {
                m_issuedHost = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            using (MemoryStream _stream = new MemoryStream())
            {
                BinaryFormatter _formatter = new BinaryFormatter();
                _formatter.Serialize(_stream, this);
                _stream.Position = 0;

                return _formatter.Deserialize(_stream);
            }
        }

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~QAgency()
        {
            Dispose(false);
        }

        #endregion IDisposable Members

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}