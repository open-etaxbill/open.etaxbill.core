using OdinSdk.OdinLib.Configuration;
using RabbitMQ.Client;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    public class FactoryQ
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address">
        /// The connection abstracts the socket connection, and takes care of protocol version negotiation and authentication and so on for us.
        /// Here we connect to a broker on the local machine - hence the localhost.
        /// If we wanted to connect to a broker on a different machine we'd simply specify its name or IP address here.
        /// </param>
        /// <param name="virtual_host">virtual host to access during this connection</param>
        /// <param name="user_name">user name</param>
        /// <param name="password">password</param>
        /// <param name="queue_name">
        /// A queue is the name for a mailbox. It lives inside RabbitMQ.
        /// Although messages flow through RabbitMQ and your applications, they can be stored only inside a queue.
        /// A queue is not bound by any limits, it can store as many messages as you like - it's essentially an infinite buffer.
        /// Many producers can send messages that go to one queue - many consumers can try to receive data from one queue.
        /// A queue will be drawn like this, with its name above it:
        /// </param>
        /// <param name="default_queue_name">default Queue Name</param>
        public FactoryQ
            (
                string host_name = null, string ip_address = null, string virtual_host = null,
                string user_name = null, string password = null, string queue_name = null,
                string default_queue_name = null
            )
        {
            __hostName = host_name ?? DefaultQ.hostName;
            __ipAddress = ip_address ?? DefaultQ.ipAddress;
            __virtualHost = virtual_host ?? DefaultQ.virtualHost;

            __userName = user_name ?? DefaultQ.userName;
            __password = password ?? DefaultQ.password;

            __queueName = queue_name ?? "";
            __defaultQName = default_queue_name ?? DefaultQueueName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="default_queue_name"></param>
        public FactoryQ(string default_queue_name)
            : this(null, null, null, null, null, null, default_queue_name)
        {
        }

        /// <summary>
        ///
        /// </summary>
        public FactoryQ()
            : this(DefaultQueueName)
        {
        }

        /// <summary>
        /// default queue 명칭 입니다.
        /// </summary>
        public const string DefaultQueueName = "default-queue";

        private string __defaultQName = null;

        /// <summary>
        ///
        /// </summary>
        public string DefaultQName
        {
            get
            {
                if (__defaultQName == null)
                    __defaultQName = DefaultQueueName;

                return __defaultQName;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public QueueElement DefaultQ
        {
            get
            {
                return MyAppService.GetQueueByName(DefaultQName);
            }
        }

        private string __hostName;

        /// <summary>
        ///
        /// </summary>
        public string HostName
        {
            get
            {
                return __hostName;
            }
            set
            {
                __hostName = value;
            }
        }

        private string __ipAddress;

        /// <summary>
        ///
        /// </summary>
        public string IpAddress
        {
            get
            {
                return __ipAddress;
            }
            set
            {
                __ipAddress = value;
            }
        }

        private string __virtualHost;

        /// <summary>
        ///
        /// </summary>
        public string VirtualHost
        {
            get
            {
                return __virtualHost;
            }
            set
            {
                __virtualHost = value;
            }
        }

        private string __userName;

        /// <summary>
        ///
        /// </summary>
        public string UserName
        {
            get
            {
                return __userName;
            }
            set
            {
                __userName = value;
            }
        }

        private string __password;

        /// <summary>
        ///
        /// </summary>
        public string Password
        {
            get
            {
                return __password;
            }
            set
            {
                __password = value;
            }
        }

        private string __queueName;

        /// <summary>
        ///
        /// </summary>
        public string QueueName
        {
            get
            {
                return __queueName;
            }
            set
            {
                __queueName = value;
            }
        }

        private ConnectionFactory __factory;

        /// <summary>
        ///
        /// </summary>
        public ConnectionFactory CFactory
        {
            get
            {
                if (__factory == null)
                {
                    __factory = new ConnectionFactory()
                    {
                        HostName = __ipAddress,
                        VirtualHost = __virtualHost,
                        UserName = __userName,
                        Password = __password
                    };
                }

                return __factory;
            }
        }
    }
}