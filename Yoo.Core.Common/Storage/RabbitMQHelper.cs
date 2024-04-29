using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Yoo.Core.Common.Storage
{
    public class RabbitMQHelper : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQHelper(string hostName, int port, string userName, string password, string vhost)
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = vhost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void DeclareExchange(string exchangeName, string exchangeType = ExchangeType.Direct)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);
        }

        public void DeclareQueue(string queueName, bool durable = false, bool exclusive = false, bool autoDelete = false)
        {
            _channel.QueueDeclare(queue: queueName,
                                  durable: durable,
                                  exclusive: exclusive,
                                  autoDelete: autoDelete,
                                  arguments: null);
        }

        public void BindQueue(string queueName, string exchangeName, string routingKey)
        {
            _channel.QueueBind(queue: queueName,
                               exchange: exchangeName,
                               routingKey: routingKey);
        }

        public void PublishMessage(string exchangeName, string queueName, string routingKey, string message)
        {
            DeclareQueue(queueName);

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: exchangeName,
                                  routingKey: routingKey,
                                  basicProperties: null,
                                  body: body);
        }

        public void ConsumeMessages(string exchangeName, string queueName, string routingKey, Action<string> handleMessage)
        {
            DeclareExchange(exchangeName);
            DeclareQueue(queueName);
            BindQueue(queueName, exchangeName, routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                handleMessage(message);
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
                _channel.Close();
            if (_connection.IsOpen)
                _connection.Close();
        }
    }
}

