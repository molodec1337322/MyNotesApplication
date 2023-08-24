using MyNotesApplication.Services.Interfaces;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace MyNotesApplication.Services.RabbitMQBroker
{
    public class PersistentConnectionRabbitMQ : IMessageBrokerPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<PersistentConnectionRabbitMQ> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private bool _disposed;

        object sync_root = new object();

        public PersistentConnectionRabbitMQ(IConfiguration configuration, ILogger<PersistentConnectionRabbitMQ> logger)
        {
            _connectionFactory = new ConnectionFactory() { HostName = configuration.GetValue<string>("RabbitMQHostAddress") };
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public bool TryConnect()
        {
            lock(sync_root)
            {
                var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_configuration.GetValue<int>("ConnectionsRetry"), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, ex.Message);
                });

                policy.Execute(() =>
                {
                     _connection = _connectionFactory.CreateConnection();
                });


                if (IsConnected)
                {
                    _connection.CallbackException += OnExceptionCallback;
                    _connection.ConnectionShutdown += OnShutdownConnection;
                    _connection.ConnectionBlocked += OnBlockedConnection;

                    _logger.LogInformation($"RabbitMQ Client acquired a persistent connection to '{_connection.Endpoint.HostName}' and is subscribed to failure events");

                    return true;
                }
                _logger.LogCritical("FATAL ERROR: cant create RabbitMQ connection");
                return false;
            }
        }

        private void OnExceptionCallback(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("RabbitMQ connection throw exception.");
            TryConnect();
        }

        private void OnShutdownConnection(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("RabbitMQ connection shutdown.");
            TryConnect();
        }

        private void OnBlockedConnection(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("RabbitMQ connection blocked.");
            TryConnect();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            try
            {
                if (_connection != null) _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }
    }
}
