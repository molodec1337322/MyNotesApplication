namespace MyNotesApplication.Services.Abstractions
{
    public abstract class Message
    {
        public string Id { get; private set; }
        public DateTime CreationTime { get; private set; }

        public Message()
        {
            Id = Guid.NewGuid().ToString();
            CreationTime = DateTime.UtcNow;
        }
    }
}
