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
    public class WorkerQ : FactoryQ
    {
        /// <summary>
        /// (2) Work queues
        /// Distributing tasks among workers
        ///
        /// Work Queues(https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html)
        ///
        /// The main idea behind Work Queues (aka: Task Queues) is to avoid doing a resource-intensive task immediately and having to wait for it to complete.
        /// Instead we schedule the task to be done later. We encapsulate a task as a message and send it to a queue.
        /// A worker process running in the background will pop the tasks and eventually execute the job.
        /// When you run many workers the tasks will be shared between them.
        ///
        /// This concept is especially useful in web applications where it's impossible to handle a complex task during a short HTTP request window.
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="virtual_host"></param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <param name="queue_name"></param>
        public WorkerQ(
                string host_name = null, string ip_address = null, string virtual_host = null,
                string user_name = null, string password = null,
                string queue_name = "durable_worker_queue"
            )
            : base(host_name, ip_address, virtual_host, user_name, password, queue_name)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intercepter"></param>
        /// <param name="timeout_ms"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async ValueTask<bool> RecvQAsync<T>(Func<T, ValueTask<bool>> intercepter, int timeout_ms = 1000, CancellationToken? cancelToken = null)
        {
            var _result = true;

            await Task.Run(() =>
            {
                using (var _connection = CFactory.CreateConnection())
                {
                    using (var _channel = _connection.CreateModel())
                    {
                        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var _consumer = new EventingBasicConsumer(_channel);

                        _consumer.Received += async (model, ea) =>
                        {
                            var _payload = ea.Body;
                            var _message = Encoding.UTF8.GetString(_payload);

                            var _value = JsonConvert.DeserializeObject<T>(_message);
                            if ((await intercepter(_value)) == true)
                                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            else
                                _result = false;
                        };

                        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: _consumer);

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
                        _channel.BasicConsume(queue: QueueName, noAck: false, consumer: _consumer);

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
                            if ((await intercepter(_value)) == false)
                            {
                                _result = false;
                                break;
                            }

                            _channel.BasicAck(deliveryTag: _evtargs.DeliveryTag, multiple: false);
                        }
                        */
                    }
                }
            });

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public async Task SendQAsync<T>(T value)
        {
            await Task.Run(() =>
            {
                using (var _connection = CFactory.CreateConnection())
                {
                    using (var _channel = _connection.CreateModel())
                    {
                        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                        var _payload = JsonConvert.SerializeObject(value);
                        var _body = Encoding.UTF8.GetBytes(_payload);

                        var _properties = _channel.CreateBasicProperties();
                        //_properties.SetPersistent(true);
                        _properties.DeliveryMode = 2;

                        _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: _properties, body: _body);
                    }
                }
            });
        }
    }
}