﻿using MyNotesApplication.Services.Interfaces;
using MyNotesApplication.Services.Message;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MyNotesApplication.Services.RabbitMQBroker
{
    public class MessageBrokerRabbitMQ : IMessageBroker
    {

        private readonly IMessageBrokerPersistentConnection _persistentConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageBrokerRabbitMQ> _logger;

        public MessageBrokerRabbitMQ(IMessageBrokerPersistentConnection persistentConnection, IConfiguration configuration, ILogger<MessageBrokerRabbitMQ> logger)
        {
            _persistentConnection = persistentConnection;
            _configuration = configuration;
            _logger = logger;
        }

        public void SendMessage(MessageWithJSONPayload message)
        {
            if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_configuration.GetValue<int>("ConnectionsRetry"), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, ex.Message);
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: _configuration.GetValue<string>("BrokerNameQueue"),
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );

                var messageToJSON = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageToJSON);

                policy.Execute(() =>
                {
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: _configuration.GetValue<string>("BrokerNameQueue"),
                        basicProperties: null,
                        body: body
                    );
                });
            }
        }
    }
}
