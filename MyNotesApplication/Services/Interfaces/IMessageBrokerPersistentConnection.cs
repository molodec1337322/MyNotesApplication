using RabbitMQ.Client;

namespace MyNotesApplication.Services.Interfaces
{
    public interface IMessageBrokerPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}
