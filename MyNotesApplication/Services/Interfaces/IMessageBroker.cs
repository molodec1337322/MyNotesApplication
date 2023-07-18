using MyNotesApplication.Services.Abstractions;

namespace MyNotesApplication.Services.Interfaces
{
    public interface IMessageBroker
    {
        void SendMessage(Message message);
    }
}
