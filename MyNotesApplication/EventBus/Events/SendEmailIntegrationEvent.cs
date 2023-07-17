using MyNotesApplication.EventBus.Events;

namespace MyNotesApplication.EventBus.IntegrationEvents
{
    public class SendEmailIntegrationEvent : IntegrationEvent
    {
        public string Email { get; }
        public string Title { get; }
        public string Body { get; }

        public SendEmailIntegrationEvent(string email, string title, string body)
        {
            Email = email;  
            Title = title;
            Body = body;
        }
    }
}
