using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//#pragma warning disable 0618

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    public class RpcQ : FactoryQ
    {
        /// <summary>
        /// (6) RPC
        /// Remote procedure call implementation
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="virtual_host"></param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <param name="queue_name"></param>
        public RpcQ(
                string host_name = null, string ip_address = null, string virtual_host = null,
                string user_name = null, string password = null,
                string queue_name = "remote_procedure_queue"
            )
            : base(host_name, ip_address, virtual_host, user_name, password, queue_name)
        {
            QConnection = CFactory.CreateConnection();
            QChannel = QConnection.CreateModel();

            __replyQueueName = QChannel.QueueDeclare();

            QConsumer = new EventingBasicConsumer(QChannel);
            QChannel.BasicConsume(__replyQueueName, true, QConsumer);
        }

        private IConnection __connection;

        /// <summary>
        ///
        /// </summary>
        public IConnection QConnection
        {
            get
            {
                return __connection;
            }
            set
            {
                __connection = value;
            }
        }

        private IModel __channel;

        /// <summary>
        ///
        /// </summary>
        public IModel QChannel
        {
            get
            {
                return __channel;
            }
            set
            {
                __channel = value;
            }
        }

        private EventingBasicConsumer __consumer;

        /// <summary>
        ///
        /// </summary>
        public EventingBasicConsumer QConsumer
        {
            get
            {
                return __consumer;
            }
            set
            {
                __consumer = value;
            }
        }

        private string __replyQueueName;

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intercepter"></param>
        /// <param name="timeout_ms"></param>
        /// <param name="tokenSource"></param>
        public async Task RecvQ<T>(Func<T, ValueTask<string>> intercepter, int timeout_ms = 1000, CancellationTokenSource tokenSource = null)
        {
            await Task.Delay(0);

            using (var _connection = CFactory.CreateConnection())
            {
                using (var _channel = _connection.CreateModel())
                {
                    _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var _consumer = new EventingBasicConsumer(_channel);
                    _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: _consumer);

                    _consumer.Received += async (model, evtargs) =>
                    {
                        if (tokenSource != null)
                        {
                            if (tokenSource.Token.IsCancellationRequested == true)
                                tokenSource.Token.ThrowIfCancellationRequested();
                        }

                        var _payload = evtargs.Body;
                        var _props = evtargs.BasicProperties;

                        var _replyProps = _channel.CreateBasicProperties();
                        _replyProps.CorrelationId = _props.CorrelationId;

                        var _response = (string)null;

                        try
                        {
                            var _message = Encoding.UTF8.GetString(_payload);

                            var _value = JsonConvert.DeserializeObject<T>(_message);
                            _response = await intercepter(_value);
                        }
                        catch (Exception)
                        {
                            _response = "";
                        }
                        finally
                        {
                            var _responseBytes = Encoding.UTF8.GetBytes(_response);
                            _channel.BasicPublish(exchange: "", routingKey: _props.ReplyTo, basicProperties: _replyProps, body: _responseBytes);

                            _channel.BasicAck(deliveryTag: evtargs.DeliveryTag, multiple: false);
                        }
                    };

                    //while (true)
                    //{
                    //    BasicDeliverEventArgs _evtargs;
                    //    if (_consumer.Queue.Dequeue(timeout_ms, out _evtargs) == false)
                    //    {
                    //        if (tokenSource != null)
                    //        {
                    //            if (tokenSource.Token.IsCancellationRequested == true)
                    //                tokenSource.Token.ThrowIfCancellationRequested();
                    //        }

                    //        continue;
                    //    }

                    //    var _payload = _evtargs.Body;
                    //    var _props = _evtargs.BasicProperties;

                    //    var _replyProps = _channel.CreateBasicProperties();
                    //    _replyProps.CorrelationId = _props.CorrelationId;

                    //    var _response = (string)null;

                    //    try
                    //    {
                    //        var _message = Encoding.UTF8.GetString(_payload);

                    //        var _value = JsonConvert.DeserializeObject<T>(_message);
                    //        _response = await intercepter(_value);
                    //    }
                    //    catch (Exception)
                    //    {
                    //        _response = "";
                    //    }
                    //    finally
                    //    {
                    //        var _responseBytes = Encoding.UTF8.GetBytes(_response);
                    //        _channel.BasicPublish(exchange: "", routingKey: _props.ReplyTo, basicProperties: _replyProps, body: _responseBytes);

                    //        _channel.BasicAck(deliveryTag: _evtargs.DeliveryTag, multiple: false);
                    //    }
                    //}
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="intercepter"></param>
        /// <param name="timeout_ms"></param>
        /// <param name="tokenSource"></param>
        public async Task SendQ<T>(T value, Func<T, ValueTask<string>> intercepter, int timeout_ms = 1000, CancellationTokenSource tokenSource = null)
        {
            await Task.Delay(0);

            var _correlationId = Guid.NewGuid().ToString();

            var _properties = QChannel.CreateBasicProperties();
            _properties.ReplyTo = __replyQueueName;
            _properties.CorrelationId = _correlationId;

            var _payload = JsonConvert.SerializeObject(value);

            var _body = Encoding.UTF8.GetBytes(_payload);
            QChannel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: _properties, body: _body);

            QConsumer.Received += async (model, evtargs) =>
            {
                if (tokenSource != null)
                {
                    if (tokenSource.Token.IsCancellationRequested == true)
                        tokenSource.Token.ThrowIfCancellationRequested();
                }

                if (evtargs.BasicProperties.CorrelationId == _correlationId)
                {
                    var _message = Encoding.UTF8.GetString(evtargs.Body);

                    var _result = JsonConvert.DeserializeObject<T>(_message);
                    await intercepter(_result);
                }
            };

            //while (true)
            //{
            //    BasicDeliverEventArgs _evtargs;
            //    if (__consumer.Queue.Dequeue(timeout_ms, out _evtargs) == false)
            //    {
            //        if (tokenSource != null)
            //        {
            //            if (tokenSource.Token.IsCancellationRequested == true)
            //                tokenSource.Token.ThrowIfCancellationRequested();
            //        }

            //        continue;
            //    }

            //    if (_evtargs.BasicProperties.CorrelationId == _correlationId)
            //    {
            //        var _message = Encoding.UTF8.GetString(_evtargs.Body);

            //        var _result = JsonConvert.DeserializeObject<T>(_message);
            //        await intercepter(_result);
            //    }
            //}
        }

        /// <summary>
        ///
        /// </summary>
        public void Close()
        {
            __connection.Close();
        }
    }
}