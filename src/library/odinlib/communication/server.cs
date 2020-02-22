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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace OdinSdk.OdinLib.Communication
{
    /// <summary>
    ///
    /// </summary>
    public class WcfServer : IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_name">서비스를 제공 할 binding name 입니다.(ex) net.tcp, BasicHttpBinding...</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="service_port">서비스를 open 할 port 번호 입니다.</param>
        public WcfServer(System.Type service_type, System.Type interface_type, string binding_name, string service_name, int service_port)
            : this(service_type, interface_type, binding_name, service_name, service_port, service_port + 1)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_names">복수의 wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="service_ports">서비스를 open 할 port 번호들 입니다.</param>
        public WcfServer(System.Type service_type, System.Type interface_type, string[] binding_names, string service_name, int[] service_ports)
            : this(service_type, interface_type, binding_names, service_name, service_ports, service_ports[service_ports.Length - 1] + 1)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_name">서비스를 제공 할 binding name 입니다.(ex) net.tcp, BasicHttpBinding...</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="service_port">서비스를 open 할 port 번호 입니다.</param>
        /// <param name="behavior_port">default 값은 service port번호에 +1 한 값입니다.</param>
        public WcfServer(System.Type service_type, System.Type interface_type, string binding_name, string service_name, int service_port, int behavior_port)
            : this(service_type, interface_type, new string[] { binding_name }, service_name, new int[] { service_port }, behavior_port)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_names">복수의 wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="service_ports">서비스를 open 할 port 번호들 입니다.</param>
        /// <param name="behavior_port">default 값은 service port번호에 +1 한 값입니다.</param>
        public WcfServer(System.Type service_type, System.Type interface_type, string[] binding_names, string service_name, int[] service_ports, int behavior_port)
            : this(service_type, interface_type, binding_names, service_name, "", service_ports, behavior_port)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_names">복수의 wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_address">서비스를 open 할 ip 주소 입니다.</param>
        /// <param name="service_ports">서비스를 open 할 port 번호들 입니다.</param>
        /// <param name="behavior_port">default 값은 service port번호에 +1 한 값입니다.</param>
        public WcfServer(
            System.Type service_type, System.Type interface_type, string[] binding_names, string service_name,
            string ip_address, int[] service_ports, int behavior_port
            )
            : this(service_type, interface_type, binding_names, service_name, ip_address, service_ports, behavior_port, false, -1)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_names">복수의 wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_address">서비스를 open 할 ip 주소 입니다.</param>
        /// <param name="service_ports">서비스를 open 할 port 번호들 입니다.</param>
        /// <param name="behavior_port">default 값은 service port번호에 +1 한 값입니다.</param>
        /// <param name="is_port_sharing">net.tcp 바인딩 인 경우 port를 공유 할 수 있습니다.</param>
        /// <param name="sharing_port">port 공유가 true인 경우 사용 되어 질 port 번호 입니다.</param>
        public WcfServer(
            System.Type service_type, System.Type interface_type, string[] binding_names, string service_name,
            string ip_address, int[] service_ports, int behavior_port,
            bool is_port_sharing, int sharing_port
            )
            : this
            (
                service_type, interface_type, binding_names, service_name,
                new string[] { ip_address }, service_ports, behavior_port,
                is_port_sharing, sharing_port
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="proxy"></param>
        public WcfServer(System.Type service_type, System.Type interface_type, WcfProxy proxy)
            : this
            (
                service_type, interface_type, proxy.BindingNames, proxy.product_name,
                proxy.IpAddresses, proxy.ServicePorts, proxy.BehaviorPort,
                proxy.IsPortSharing, proxy.SharingPort
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_type">wcf service를 제공 할 host service의 class type</param>
        /// <param name="interface_type">지정된 계약이 구현되어 있는 interface type</param>
        /// <param name="binding_names">복수의 wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_addresses">서비스를 open 할 ip 주소 입니다.</param>
        /// <param name="service_ports">서비스를 open 할 port 번호들 입니다.</param>
        /// <param name="behavior_port">default 값은 service port번호에 +1 한 값입니다.</param>
        /// <param name="is_port_sharing">net.tcp 바인딩 인 경우 port를 공유 할 수 있습니다.</param>
        /// <param name="sharing_port">port 공유가 true인 경우 사용 되어 질 port 번호 입니다.</param>
        public WcfServer(
            System.Type service_type, System.Type interface_type, string[] binding_names, string service_name,
            string[] ip_addresses, int[] service_ports, int behavior_port,
            bool is_port_sharing, int sharing_port
            )
        {
            ServiceType = service_type;
            InterfaceType = interface_type;

            BindingNames = binding_names;
            ServiceName = service_name;

            if (String.IsNullOrEmpty(ip_addresses[0]) == true)
            {
                IpAddresses = CfgHelper.SNG.GetIPAddresses();
                AnyIpAddress = true;
            }
            else
            {
                IpAddresses = ip_addresses;
                AnyIpAddress = false;
            }

            HostName = System.Net.Dns.GetHostName();

            ServicePorts = service_ports;
            BehaviorPort = behavior_port;

            IsPortSharing = is_port_sharing;
            SharingPort = sharing_port;

            m_readerQuotas = new Lazy<XmlDictionaryReaderQuotas>(GetReaderQuotas);
            m_serverHost = new Lazy<ServiceHost>(GetServiceHost);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private XmlDictionaryReaderQuotas GetReaderQuotas()
        {
            XmlDictionaryReaderQuotas _readerQuotas = new XmlDictionaryReaderQuotas();
            {
                _readerQuotas.MaxArrayLength *= 10;
                _readerQuotas.MaxBytesPerRead *= 10;
                _readerQuotas.MaxDepth *= 10;
                _readerQuotas.MaxNameTableCharCount *= 10;
                _readerQuotas.MaxStringContentLength = 1024 * 1024 * 5;
            }

            return _readerQuotas;
        }

        private ServiceHost GetServiceHost()
        {
            return new ServiceHost(ServiceType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private Lazy<ServiceHost> m_serverHost;

        /// <summary>
        ///
        /// </summary>
        public ServiceHost ServerHost
        {
            get
            {
                return m_serverHost.Value;
            }
            set
            {
                m_serverHost = new Lazy<ServiceHost>(() =>
                {
                    return value;
                });
            }
        }

        /// <summary>
        ///
        /// </summary>
        public System.Type ServiceType
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public System.Type InterfaceType
        {
            get;
            set;
        }

        /// <summary>
        /// WCF가 서비스를 open 할 binding name들 입니다.
        /// </summary>
        public string[] BindingNames
        {
            get;
            set;
        }

        /// <summary>
        /// server에서 서비스 할 명칭 입니다.
        /// </summary>
        public string ServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// ip 주소들
        /// </summary>
        public string[] IpAddresses
        {
            get;
            set;
        }

        /// <summary>
        /// 여러개의 IP를 사용 하는지 여부
        /// </summary>
        public bool AnyIpAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Host Name
        /// </summary>
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// WCF server에서 open 할 port 번호들 입니다.
        /// </summary>
        public int[] ServicePorts
        {
            get;
            set;
        }

        /// <summary>
        /// client가 reference시 WSDL을 제공하는 port 입니다.
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
        ///
        /// </summary>
        public string BehaviorUrl
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string WcfAddress
        {
            get;
            set;
        }

        private int m_maxReceivedMessageSize = 20;

        /// <summary>
        ///
        /// </summary>
        public int MaxReceivedMessageSize
        {
            get
            {
                return m_maxReceivedMessageSize;
            }
            set
            {
                m_maxReceivedMessageSize = value;
            }
        }

        private int m_maxBufferPoolSize = 2;

        /// <summary>
        ///
        /// </summary>
        public int MaxBufferPoolSize
        {
            get
            {
                return m_maxBufferPoolSize;
            }
            set
            {
                m_maxBufferPoolSize = value;
            }
        }

        private Lazy<XmlDictionaryReaderQuotas> m_readerQuotas;

        /// <summary>
        ///
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return m_readerQuotas.Value;
            }
            set
            {
                m_readerQuotas = new Lazy<XmlDictionaryReaderQuotas>(() =>
                {
                    return value;
                });
            }
        }

        private Lazy<TimeSpan> m_sendTimeout = new Lazy<TimeSpan>(() => TimeSpan.FromDays(7));

        /// <summary>
        ///
        /// </summary>
        public TimeSpan SendTimeout
        {
            get
            {
                return m_sendTimeout.Value;
            }
            set
            {
                m_sendTimeout = new Lazy<TimeSpan>(() =>
                {
                    return value;
                });
            }
        }

        private Lazy<TimeSpan> m_receiveTimeout = new Lazy<TimeSpan>(() => TimeSpan.FromDays(7));

        /// <summary>
        ///
        /// </summary>
        public TimeSpan ReceiveTimeout
        {
            get
            {
                return m_receiveTimeout.Value;
            }
            set
            {
                m_receiveTimeout = new Lazy<TimeSpan>(() =>
                {
                    return value;
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        private static object m_syncRoot = null;

        /// <summary>
        /// 액세스를 동기화하는 데 사용할 수 있는 개체를 가져옵니다.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (m_syncRoot == null)
                    m_syncRoot = new object();

                return m_syncRoot;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Start()
        {
            if (AnyIpAddress == false)
            {
                for (int _ipadrs = 0; _ipadrs < IpAddresses.Length; _ipadrs++)
                    this.Start(IpAddresses[_ipadrs]);
            }
            else
            {
                this.Start(HostName);
            }

            var _ipOrLocalHost = (AnyIpAddress == false) ? IpAddresses[0] : "localhost";
            {
                var _urlBehavior = String.Format("http://{0}:{1}/{2}", _ipOrLocalHost, BehaviorPort, ServiceName);

                ServiceThrottlingBehavior _throttling = ServerHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (_throttling == null)
                {
                    _throttling = new ServiceThrottlingBehavior
                    {
                        MaxConcurrentCalls = int.MaxValue,
                        MaxConcurrentSessions = int.MaxValue,
                        MaxConcurrentInstances = int.MaxValue
                    };

                    ServerHost.Description.Behaviors.Add(_throttling);
                }

                //ServiceDebugBehavior _debugging = ServerHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                //if (_debugging == null)
                //{
                //    _debugging = new ServiceDebugBehavior();
                //    _debugging.IncludeExceptionDetailInFaults = true;

                //    ServerHost.Description.Behaviors.Add(_debugging);
                //}

                ServiceMetadataBehavior _metadata = ServerHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (_metadata == null)
                {
                    _metadata = new ServiceMetadataBehavior
                    {
                        HttpGetUrl = new Uri(_urlBehavior),
                        HttpGetEnabled = true
                    };

                    ServerHost.Description.Behaviors.Add(_metadata);
                }

                this.BehaviorUrl = _urlBehavior;
                ServerHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), _urlBehavior);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address_or_host_name"></param>
        public void Start(string ip_address_or_host_name)
        {
            for (int _bind = 0; _bind < BindingNames.Length; _bind++)
            {
                var _binding_name = BindingNames[_bind];
                var _servicePort = ServicePorts[_bind];

                this.Start(ip_address_or_host_name, _binding_name, _servicePort);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address_or_host_name"></param>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        public void Start(string ip_address_or_host_name, string binding_name, int service_port)
        {
            lock (SyncRoot)
            {
                Binding _binding;
                var _address = String.Format("://{0}:{1}/{2}", ip_address_or_host_name, service_port, ServiceName);

                if (binding_name.ToLower() == "WSHttpBinding".ToLower())
                {
                    _address = "http" + _address;

                    WSHttpBinding _binding2 = new WSHttpBinding();
                    {
                        _binding2.TransactionFlow = false;

                        _binding2.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                        _binding2.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                        _binding2.Security.Mode = SecurityMode.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                else if (binding_name.ToLower() == "WSDualHttpBinding".ToLower())
                {
                    _address = "http" + _address;

                    WSDualHttpBinding _binding2 = new WSDualHttpBinding();
                    {
                        _binding2.TransactionFlow = false;

                        _binding2.Security.Message.ClientCredentialType = MessageCredentialType.None;
                        _binding2.Security.Mode = WSDualHttpSecurityMode.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                else if (binding_name.ToLower() == "NetMsmqBinding".ToLower())
                {
                    _address = String.Format("net.msmq://{0}/private/{1}", ip_address_or_host_name, ServiceName);

                    NetMsmqBinding _binding2 = new NetMsmqBinding();
                    {
                        _binding2.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
                        _binding2.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.SendTimeout = SendTimeout;
                        _binding2.ReceiveTimeout = ReceiveTimeout;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                else if (binding_name.ToLower() == "NetNamedPipeBinding".ToLower())
                {
                    _address = "net.pipe" + _address;

                    NetNamedPipeBinding _binding2 = new NetNamedPipeBinding();
                    {
                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.SendTimeout = SendTimeout;
                        _binding2.ReceiveTimeout = ReceiveTimeout;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                //else if (_binding_name.ToLower() == "NetPeerTcpBinding".ToLower())
                //{
                //    _address = "net.peer" + _address;

                //    NetPeerTcpBinding _binding2 = new NetPeerTcpBinding();
                //    {
                //        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                //        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                //        _binding2.SendTimeout = SendTimeout;
                //        _binding2.ReceiveTimeout = ReceiveTimeout;

                //        _binding2.ReaderQuotas = ReaderQuotas;
                //    }

                //    _binding = _binding2;
                //}
                else if (binding_name.ToLower() == "net.tcp".ToLower())
                {
                    if (IsPortSharing == true)
                        service_port = SharingPort;

                    _address = String.Format("net.tcp://{0}:{1}/{2}", ip_address_or_host_name, service_port, ServiceName);

                    NetTcpBinding _binding2 = new NetTcpBinding();
                    {
                        _binding2.Security.Mode = SecurityMode.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.SendTimeout = SendTimeout;
                        _binding2.ReceiveTimeout = ReceiveTimeout;

                        _binding2.ReaderQuotas = ReaderQuotas;
                        _binding2.PortSharingEnabled = IsPortSharing;
                    }

                    _binding = _binding2;
                }
                else if (binding_name.ToLower() == "BasicHttpBinding".ToLower())
                {
                    _address = "http" + _address;

                    BasicHttpBinding _binding2 = new BasicHttpBinding();
                    {
                        _binding2.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;

                        _binding2.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                        _binding2.Security.Mode = BasicHttpSecurityMode.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                else if (binding_name.ToLower() == "WebHttpBinding".ToLower())
                {
                    _address = "http" + _address;

                    WebHttpBinding _binding2 = new WebHttpBinding();
                    {
                        _binding2.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;

                        _binding2.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                        _binding2.Security.Mode = WebHttpSecurityMode.None;

                        _binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        _binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }
                else
                {
                    _address = "http" + _address;

                    // Create a custom binding that contains two binding elements.
                    ReliableSessionBindingElement _reliableSession = new ReliableSessionBindingElement
                    {
                        InactivityTimeout = TimeSpan.FromDays(7),
                        Ordered = true
                    };

                    HttpTransportBindingElement _httpTransport = new HttpTransportBindingElement
                    {
                        AuthenticationScheme = System.Net.AuthenticationSchemes.Anonymous,
                        HostNameComparisonMode = HostNameComparisonMode.StrongWildcard
                    };

                    CustomBinding _binding2 = new CustomBinding(_reliableSession, _httpTransport);
                    {
                        //_binding2.MaxReceivedMessageSize *= MaxReceivedMessageSize;
                        //_binding2.MaxBufferPoolSize *= MaxBufferPoolSize;

                        _binding2.SendTimeout = SendTimeout;
                        _binding2.ReceiveTimeout = ReceiveTimeout;

                        //_binding2.ReaderQuotas = ReaderQuotas;
                    }

                    _binding = _binding2;
                }

                this.WcfAddress += _address + " ";
                ServerHost.AddServiceEndpoint(InterfaceType, _binding, _address);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Stop()
        {
            lock (SyncRoot)
            {
                if (ServerHost != null)
                {
                    if (ServerHost.State != CommunicationState.Faulted)
                    {
                        ServerHost.Close(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        ServerHost.Abort();
                    }

                    ServerHost = null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

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
                Stop();

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~WcfServer()
        {
            Dispose(false);
        }

        #endregion IDisposable Members

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}