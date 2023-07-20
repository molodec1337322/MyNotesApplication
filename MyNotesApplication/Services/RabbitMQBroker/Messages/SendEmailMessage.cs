namespace MyNotesApplication.Services.RabbitMQBroker.Messages
{
    public class SendEmailMessage
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public SendEmailMessage(string Email, string Subject, string Body) 
        { 
            this.Email = Email;
            this.Subject = Subject; 
            this.Body = Body;
        }
    }
}
