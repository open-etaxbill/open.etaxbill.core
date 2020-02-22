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
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace OdinSdk.OdinLib.Communication
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WcfClient<T>
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        public WcfClient(string binding_name, string service_name, int service_port)
            : this(binding_name, service_name, "", service_port)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        public WcfClient(string binding_name, string service_name, string ip_address, int service_port)
            : this(binding_name, service_name, ip_address, service_port, true, true)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        /// <param name="retry_after_exception"></param>
        /// <param name="recreate_on_fault"></param>
        public WcfClient(string binding_name, string service_name, string ip_address, int service_port, bool retry_after_exception = true, bool recreate_on_fault = true)
            : this
            (
                binding_name, service_name, ip_address, service_port, retry_after_exception, recreate_on_fault, false, -1
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="retry_after_exception"></param>
        /// <param name="recreate_on_fault"></param>
        public WcfClient(WcfProxy proxy, bool retry_after_exception = true, bool recreate_on_fault = true)
            : this
            (
                proxy.BindingName, proxy.product_name, proxy.IpAddress, proxy.ServicePort,
                retry_after_exception, recreate_on_fault, proxy.IsPortSharing, proxy.SharingPort
            )
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="binding_name">wcf 바인딩 명칭 (ex)net.tcp</param>
        /// <param name="service_name">wcf서비스 명칭</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="service_port">wcf접속시 연결 할 port번호</param>
        /// <param name="retry_after_exception"></param>
        /// <param name="recreate_on_fault"></param>
        /// <param name="is_port_sharing">포트 공유를 하는지 여부(port sharing service가 동작 중이어야 함)</param>
        /// <param name="sharing_port">wcf가 포트를 공유 할 포트 번호</param>
        public WcfClient(
            string binding_name, string service_name, string ip_address, int service_port,
            bool retry_after_exception, bool recreate_on_fault,
            bool is_port_sharing = false, int sharing_port = -1
            )
        {
            BindingName = binding_name;
            ServiceName = service_name;

            if (String.IsNullOrEmpty(ip_address) == true)
                IpAddress = CfgHelper.SNG.GetIPAddress();
            else
                IpAddress = ip_address;

            ServicePort = service_port;

            RetryAfterException = retry_after_exception;
            RecreateOnFault = recreate_on_fault;

            IsPortSharing = is_port_sharing;
            SharingPort = sharing_port;

            m_readerQuotas = new Lazy<XmlDictionaryReaderQuotas>(GetReaderQuotas);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private XmlDictionaryReaderQuotas GetReaderQuotas()
        {
            var _readerQuotas = new XmlDictionaryReaderQuotas();
            {
                _readerQuotas.MaxArrayLength *= 10;
                _readerQuotas.MaxBytesPerRead *= 10;
                _readerQuotas.MaxDepth *= 10;
                _readerQuotas.MaxNameTableCharCount *= 10;
                _readerQuotas.MaxStringContentLength = 1024 * 1024 * 5;
            }

            return _readerQuotas;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// WCF server에 연결 할 binding name 입니다.
        /// ex) net.tcp, BasicHttpBinding
        /// </summary>
        public string BindingName
        {
            get;
            set;
        }

        /// <summary>
        /// WCF server에서 서비스 되고 있는 명칭 입니다.
        /// </summary>
        public string ServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// ip 주소
        /// </summary>
        public string IpAddress
        {
            get;
            set;
        }

        /// <summary>
        /// WCF server에 접속 할 port 번호 입니다.
        /// 주의점) 바인딩명과 상이한 port번호인지 확인 하여야 합니다.
        /// </summary>
        public int ServicePort
        {
            get;
            set;
        }

        /// <summary>
        /// 오류 발생시 재 시도 횟수
        /// </summary>
        public bool RetryAfterException
        {
            get;
            set;
        }

        /// <summary>
        /// fault시 재 생성 여부
        /// </summary>
        public bool RecreateOnFault
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
        public string WcfAddress
        {
            get;
            set;
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

        private ChannelFactory<T> m_channelFactory = null;
        private T m_innerChannel = default(T);

        /// <summary>
        ///
        /// </summary>
        public T InnerChannel
        {
            get
            {
                lock (SyncRoot)
                {
                    if (Object.Equals(m_innerChannel, default(T)) == false)
                    {
                        ICommunicationObject _proxy = m_innerChannel as ICommunicationObject;

                        if (_proxy.State == CommunicationState.Faulted && RecreateOnFault == true)
                        {
                            // channel has been faulted, we want to create a new one so clear it
                            m_innerChannel = default(T);
                        }
                    }

                    if (Object.Equals(m_innerChannel, default(T)) == true)
                    {
                        // channel is null, create a new one
                        Debug.Assert(m_channelFactory != null);     // could not call start() function here because be locking
                        m_innerChannel = m_channelFactory.CreateChannel();
                    }
                }

                return m_innerChannel;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
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

        /// <summary>
        ///
        /// </summary>
        public void Start()
        {
            lock (SyncRoot)
            {
                Binding _binding;
                var _servicePort = ServicePort;

                var _address = String.Format("://{0}:{1}/{2}", IpAddress, _servicePort, ServiceName);

                if (BindingName.ToLower() == "WSHttpBinding".ToLower())
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
                else if (BindingName.ToLower() == "WSDualHttpBinding".ToLower())
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
                else if (BindingName.ToLower() == "NetMsmqBinding".ToLower())
                {
                    _address = String.Format("net.msmq://{0}/private/{1}", IpAddress, ServiceName);

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
                else if (BindingName.ToLower() == "NetNamedPipeBinding".ToLower())
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
                //else if (BindingName.ToLower() == "NetPeerTcpBinding".ToLower())
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
                else if (BindingName.ToLower() == "net.tcp".ToLower())
                {
                    if (IsPortSharing == true)
                        _servicePort = SharingPort;

                    _address = String.Format("net.tcp://{0}:{1}/{2}", IpAddress, _servicePort, ServiceName);

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
                else if (BindingName.ToLower() == "BasicHttpBinding".ToLower())
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
                else if (BindingName.ToLower() == "WebHttpBinding".ToLower())
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
                    }

                    _binding = _binding2;
                }

                this.WcfAddress = _address;
                m_channelFactory = new ChannelFactory<T>(_binding, new EndpointAddress(_address));
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Stop()
        {
            lock (SyncRoot)
            {
                if (Object.Equals(m_innerChannel, default(T)) == false)
                {
                    ICommunicationObject _proxy = m_innerChannel as ICommunicationObject;
                    try
                    {
                        if (_proxy != null)
                        {
                            if (_proxy.State != CommunicationState.Faulted)
                            {
                                _proxy.Close(TimeSpan.FromSeconds(1));
                            }
                            else
                            {
                                _proxy.Abort();
                            }
                        }
                    }
                    catch (CommunicationException)
                    {
                        _proxy.Abort();
                    }
                    catch (TimeoutException)
                    {
                        _proxy.Abort();
                    }
                    catch (Exception ex)
                    {
                        _proxy.Abort();
                        throw ex;
                    }
                    finally
                    {
                        m_innerChannel = default(T);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}