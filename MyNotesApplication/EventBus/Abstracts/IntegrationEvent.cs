namespace MyNotesApplication.EventBus.Events
{
    public abstract class IntegrationEvent
    {
        public string Id { get; }
        public DateTime CreationDate { get; }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow;
        }
    }
}
