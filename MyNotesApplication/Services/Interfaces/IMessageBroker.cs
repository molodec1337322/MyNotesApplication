using MyNotesApplication.Services.Message;

namespace MyNotesApplication.Services.Interfaces
{
    public interface IMessageBroker
    {
        void SendMessage(MessageWithJSONPayload message);
    }
}
