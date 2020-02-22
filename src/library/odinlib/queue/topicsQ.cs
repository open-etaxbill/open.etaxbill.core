using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    public class TopicsQ : FactoryQ
    {
        /// <summary>
        /// (5) Topics
        /// Receiving messages based on a pattern
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="virtual_host"></param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <param name="exchange_name"></param>
        /// <param name="topic"></param>
        public TopicsQ(
                string host_name = null, string ip_address = null, string virtual_host = null,
                string user_name = null, string password = null,
                string exchange_name = "topics_logging_exchange", string topic = "info.*"
            )
            : base(host_name, ip_address, virtual_host, user_name, password)
        {
            __exchangeName = exchange_name;

            var _topics = topic.Split(';');
            if (_topics.Length > 0)
                __topics = _topics;
        }

        private string __exchangeName;

        /// <summary>
        ///
        /// </summary>
        public string ExchangeName
        {
            get
            {
                return __exchangeName;
            }
            set
            {
                __exchangeName = value;
            }
        }

        private string[] __topics;

        /// <summary>
        ///
        /// </summary>
        public string[] Topics
        {
            get
            {
                return __topics;
            }
            set
            {
                __topics = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intercepter"></param>
        /// <param name="timeout_ms"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public bool RecvQ<T>(Func<T, ValueTask<bool>> intercepter, int timeout_ms = 1000, CancellationToken? cancelToken = null)
        {
            var _result = true;

            using (var _connection = CFactory.CreateConnection())
            {
                using (var _channel = _connection.CreateModel())
                {
                    _channel.ExchangeDeclare(exchange: ExchangeName, type: "topic");

                    var _queueName = _channel.QueueDeclare().QueueName;
                    foreach (var _topic in Topics)
                        _channel.QueueBind(queue: _queueName, exchange: ExchangeName, routingKey: _topic);

                    var _consumer = new EventingBasicConsumer(_channel);

                    _consumer.Received += async (model, ea) =>
                    {
                        var _payload = ea.Body;
                        var _message = Encoding.UTF8.GetString(_payload);

                        var _value = JsonConvert.DeserializeObject<T>(_message);
                        if ((await intercepter(_value)) == false)
                            _result = false;
                    };

                    _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: _consumer);

                    while (true)
                    {
                        if (cancelToken != null)
                        {
                            var _cancelled = cancelToken.Value.WaitHandle.WaitOne(timeout_ms * 1000);
                            if (_cancelled == true)
                                break;
                        }
                        else
                        {
                            Thread.Sleep(timeout_ms * 1000);
                        }
                    }

                    /*
                    var _consumer = new EventingBasicConsumer(_channel);
                    _channel.BasicConsume(queue: _queueName, noAck: true, consumer: _consumer);

                    while (true)
                    {
                        BasicDeliverEventArgs _evtargs;
                        if (_consumer.Queue.Dequeue(timeout_ms, out _evtargs) == false)
                        {
                            if (cancelToken != null)
                            {
                                if (cancelToken.Value.IsCancellationRequested == true)
                                    cancelToken.Value.ThrowIfCancellationRequested();
                            }

                            continue;
                        }

                        var _payload = _evtargs.Body;
                        var _message = Encoding.UTF8.GetString(_payload);

                        var _value = JsonConvert.DeserializeObject<T>(_message);
                        if (await intercepter(_value) == false)
                        {
                            _result = false;
                            break;
                        }
                    }
                    */
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="topic"></param>
        public void SendQ<T>(T value, string topic = null)
        {
            var _topic = topic ?? Topics[0];

            using (var _connection = CFactory.CreateConnection())
            {
                using (var _channel = _connection.CreateModel())
                {
                    _channel.ExchangeDeclare(exchange: ExchangeName, type: "topic");

                    var _payload = JsonConvert.SerializeObject(value);
                    var _body = Encoding.UTF8.GetBytes(_payload);

                    _channel.BasicPublish(exchange: ExchangeName, routingKey: _topic, basicProperties: null, body: _body);
                }
            }
        }
    }
}