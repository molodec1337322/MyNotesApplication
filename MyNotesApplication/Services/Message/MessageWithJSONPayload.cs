namespace MyNotesApplication.Services.Message
{
    public class MessageWithJSONPayload
    {
        public string Id { get; private set; }
        public DateTime CreationTime { get; private set; }
        public string ServiceName { get; private set; }

        public string JSONPayload { get; set; }

        public MessageWithJSONPayload(string serviceName, string jSONPayload)
        {
            Id = Guid.NewGuid().ToString();
            CreationTime = DateTime.UtcNow;
            ServiceName = serviceName;
            JSONPayload = jSONPayload;
        }
    }
}
