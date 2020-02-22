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
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Communication
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class WcfProxy : ICloneable, IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="certification_app"></param>
        /// <param name="app_connection"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        public WcfProxy(
            string description, string binding_name, int service_port, int behavior_port, string soap_url,
            string event_source, string certification_app, string app_connection, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version
        )
            : this
            (
                description, binding_name, "", service_port, behavior_port, soap_url,
                event_source, certification_app, app_connection, configuration_name,
                protocol_name, category_id, product_id, product_name, product_version
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="certification_app"></param>
        /// <param name="app_connection"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        public WcfProxy(
            string description, string binding_name, string ip_address, int service_port, int behavior_port, string soap_url,
            string event_source, string certification_app, string app_connection, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version
        )
            : this
            (
                description, new string[] { binding_name }, ip_address, new int[] { service_port }, behavior_port, soap_url,
                event_source, certification_app, app_connection, configuration_name,
                protocol_name, category_id, product_id, product_name, product_version
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_names">binding name (ex)net.tcp</param>
        /// <param name="service_ports"></param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="certification_app"></param>
        /// <param name="app_connection"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        public WcfProxy(
            string description, string[] binding_names, int[] service_ports, int behavior_port, string soap_url,
            string event_source, string certification_app, string app_connection, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version
        )
            : this
            (
                description, binding_names, "", service_ports, behavior_port, soap_url,
                event_source, certification_app, app_connection, configuration_name,
                protocol_name, category_id, product_id, product_name, product_version
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_names">binding name (ex)net.tcp</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_ports"></param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="certification_app"></param>
        /// <param name="app_connection"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        public WcfProxy(
            string description, string[] binding_names, string ip_address, int[] service_ports, int behavior_port, string soap_url,
            string event_source, string certification_app, string app_connection, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version
        )
            : this
            (
                description, binding_names, new string[] { ip_address }, service_ports, behavior_port, soap_url,
                event_source, certification_app, app_connection, configuration_name,
                protocol_name, category_id, product_id, product_name, product_version
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_names">binding name (ex)net.tcp</param>
        /// <param name="ip_addresses">ip-address</param>
        /// <param name="service_ports"></param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="application_id"></param>
        /// <param name="connection_name"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        public WcfProxy(
            string description, string[] binding_names, string[] ip_addresses, int[] service_ports, int behavior_port, string soap_url,
            string event_source, string application_id, string connection_name, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version
        )
            : this
            (
                description, binding_names, ip_addresses, service_ports, behavior_port, soap_url,
                event_source, application_id, connection_name, configuration_name,
                protocol_name, category_id, product_id, product_name, product_version, false, -1
            )
        {
        }

        /// <summary>
        /// ?ы듃怨듭쑀瑜?吏???⑸땲??
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_names">binding name (ex)net.tcp</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_ports"></param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="application_id"></param>
        /// <param name="connection_name"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name">MSMQ Open???ъ슜 ?섎뒗 ?꾨줈?좎퐳 紐낆묶(ex)TCP,HTTP</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="is_port_sharing">포트 공유를 하는지 여부(port sharing service가 동작 중이어야 함)</param>
        /// <param name="sharing_port">wcf가 포트를 공유 할 포트 번호</param>
        public WcfProxy(
            string description, string[] binding_names, string ip_address, int[] service_ports, int behavior_port, string soap_url,
            string event_source, string application_id, string connection_name, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version, bool is_port_sharing, int sharing_port
        )
            : this
            (
            description, binding_names, new string[] { ip_address }, service_ports, behavior_port, soap_url,
            event_source, application_id, connection_name, configuration_name,
            protocol_name, category_id, product_id, product_name, product_version, is_port_sharing, sharing_port
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="description">Remoting Service Name</param>
        /// <param name="binding_names">binding name (ex)net.tcp</param>
        /// <param name="ip_addresses">ip-address</param>
        /// <param name="service_ports"></param>
        /// <param name="behavior_port"></param>
        /// <param name="soap_url"></param>
        /// <param name="event_source"></param>
        /// <param name="application_id"></param>
        /// <param name="connection_name"></param>
        /// <param name="configuration_name"></param>
        /// <param name="protocol_name"></param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="is_port_sharing">포트 공유를 하는지 여부(port sharing service가 동작 중이어야 함)</param>
        /// <param name="sharing_port">wcf가 포트를 공유 할 포트 번호</param>
        public WcfProxy(
            string description, string[] binding_names, string[] ip_addresses, int[] service_ports, int behavior_port, string soap_url,
            string event_source, string application_id, string connection_name, string configuration_name,
            string protocol_name, string category_id, string product_id, string product_name, string product_version, bool is_port_sharing, int sharing_port
        )
        {
            this.Description = description;
            this.BindingNames = binding_names;

            this.ServicePorts = service_ports;
            this.BehaviorPort = behavior_port;
            this.SoapUrl = soap_url;
            this.EventSource = event_source;

            this.ApplicationId = application_id;
            this.ConnectionName = connection_name;
            this.ConfigurationName = configuration_name;

            this.QProtocolName = protocol_name;

            this.CategoryId = category_id;
            this.ProductId = product_id;
            this.product_name = product_name;
            this.pVersion = product_version;

            this.IsPortSharing = is_port_sharing;
            this.SharingPort = sharing_port;

            if (String.IsNullOrEmpty(ip_addresses[0]) == true)
                ip_addresses[0] = RegHelper.SNG.GetIpAddress(HostName);

            this.IpAddresses = ip_addresses;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <example>Remoting Service Name</example>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <example>"7b7eb9e0-9a8b-4c1a-aec4-61dad153a6da"</example>
        public string ApplicationId
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <example>APP_CONN</example>
        public string ConnectionName
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <example>DataAccess.config</example>
        public string ConfigurationName
        {
            get;
            set;
        }

        private Lazy<string[]> m_binding_names = new Lazy<string[]>(() => new string[] { "WSHttpBinding" });

        /// <summary>
        ///
        /// </summary>
        public string[] BindingNames
        {
            get
            {
                return m_binding_names.Value;
            }
            set
            {
                m_binding_names = new Lazy<string[]>(() =>
                {
                    return value;
                });
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string BindingName
        {
            get
            {
                return BindingNames[0];
            }
        }

        /// <summary>
        /// MSMQ Protocol
        /// </summary>
        /// <example>TCP</example>
        public string QProtocolName
        {
            get;
            set;
        }

        /// <summary>
        /// categoryId
        /// </summary>
        /// <example>sdk</example>
        public string CategoryId
        {
            get;
            set;
        }

        /// <summary>
        /// productId
        /// </summary>
        /// <example>DataAccess</example>
        public string ProductId
        {
            get;
            set;
        }

        /// <summary>
        /// product_name
        /// </summary>
        public string product_name
        {
            get;
            set;
        }

        /// <summary>
        /// product version
        /// </summary>
        public string pVersion
        {
            get;
            set;
        }

        /// <summary>
        /// L4 사용 여부
        /// </summary>
        public string UsingL4
        {
            get;
            set;
        }

        /// <summary>
        /// L4 IpAddress
        /// </summary>
        public string IpAddressL4
        {
            get;
            set;
        }

        private Lazy<string> m_host_name = new Lazy<string>(() => System.Environment.MachineName);

        /// <summary>
        ///
        /// </summary>
        public string HostName
        {
            get
            {
                return m_host_name.Value;
            }
            set
            {
                m_host_name = new Lazy<string>(() =>
                {
                    return value;
                });
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Type svObject
        {
            get;
            set;
        }

        private Lazy<int[]> m_ports = new Lazy<int[]>(() => new int[] { 8000 });

        /// <summary>
        ///
        /// </summary>
        public int[] ServicePorts
        {
            get
            {
                return m_ports.Value;
            }
            set
            {
                m_ports = new Lazy<int[]>(() =>
                {
                    return value;
                });
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string[] IpAddresses
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string IpAddress
        {
            get
            {
                return IpAddresses[0];
            }
            set
            {
                IpAddresses[0] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int ServicePort
        {
            get
            {
                return ServicePorts[0];
            }
        }

        /// <summary>
        /// 8001
        /// </summary>
        public int BehaviorPort
        {
            get;
            set;
        }

        /// <summary>
        /// 포트 공유를 할 것인지 여부를 지정 합니다.
        /// 포트 공유는 net.tcp 프로토콜만 해당 됩니다.
        /// </summary>
        public bool IsPortSharing
        {
            get;
            set;
        }

        /// <summary>
        /// 포트 공유가 true인 경우 공유 되어질 포트 번호 입니다.
        /// </summary>
        public int SharingPort
        {
            get;
            set;
        }

        /// <summary>
        /// RemoteConstant.soap
        /// </summary>
        public string SoapUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Remoting Event Source Name
        /// </summary>
        public string EventSource
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 호스트명으로 ip 주소를 얻는다. ip주소는 shared 아래에만 위치 해야 함
        /// </summary>
        /// <returns></returns>
        public string GetClientIpAddressByConfigurationName()
        {
            var _result = RegHelper.SNG.GetClient(this.CategoryId, this.ProductId, this.ConfigurationName, "");
            if (String.IsNullOrEmpty(_result) == true)
            {
                _result = CfgHelper.SNG.GetAppString(this.ConfigurationName);
                RegHelper.SNG.SetClient(this.CategoryId, this.ProductId, this.ConfigurationName, _result);
            }

            return RegHelper.SNG.GetRedirectedIpAddress(_result);
        }

        /// <summary>
        ///
        /// </summary>
        public void SetClientIpAddressByConfigurationName()
        {
            var _ip_address = GetClientIpAddressByConfigurationName();
            if (String.IsNullOrEmpty(_ip_address) == false)
                this.IpAddress = _ip_address;
        }

        /// <summary>
        /// 호스트명으로 ip 주소를 얻는다. ip주소는 shared 아래에만 위치 해야 함
        /// </summary>
        /// <returns></returns>
        public string GetServerIpAddressByConfigurationName()
        {
            var _result = RegHelper.SNG.GetServer(this.CategoryId, this.ProductId, this.ConfigurationName, "");
            if (String.IsNullOrEmpty(_result) == true)
            {
                _result = CfgHelper.SNG.GetAppString(this.ConfigurationName);
                RegHelper.SNG.SetServer(this.CategoryId, this.ProductId, this.ConfigurationName, _result);
            }

            return RegHelper.SNG.GetRedirectedIpAddress(_result);
        }

        /// <summary>
        ///
        /// </summary>
        public void SetServerIpAddressByConfigurationName()
        {
            var _ip_address = GetServerIpAddressByConfigurationName();
            if (String.IsNullOrEmpty(_ip_address) == false)
                this.IpAddress = _ip_address;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 포트 공유를 지정한다. Registry에 정의된 값을 사용 합니다.
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        private void SetPortSharing(string ip_address)
        {
            var _ip_address = ip_address;
            {
                if (String.IsNullOrEmpty(_ip_address) == true)
                    _ip_address = CfgHelper.SNG.IPAddress;

                _ip_address = RegHelper.SNG.GetRedirectedIpAddress(_ip_address);
            }

            IsPortSharing = RegHelper.SNG.GetIsPortSharing(_ip_address);
            SharingPort = RegHelper.SNG.GetSharingPort(_ip_address);
        }

        /// <summary>
        /// 서버 측 포트 공유를 지정한다.
        /// </summary>
        public void SetServerPortSharing(string ip_address = "")
        {
            SetPortSharing(ip_address);
        }

        /// <summary>
        /// 클라이언트 측 포트 공유를 지정한다.
        /// </summary>
        /// <param name="ip_address">포트 공유 할 ip 주소</param>
        public void SetClientPortSharing(string ip_address = "")
        {
            SetPortSharing(ip_address);
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
        ~WcfProxy()
        {
            Dispose(false);
        }

        #endregion IDisposable Members

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}