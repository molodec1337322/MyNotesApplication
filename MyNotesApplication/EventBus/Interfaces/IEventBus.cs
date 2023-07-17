namespace MyNotesApplication.Services.Interfaces
{
    public interface IEventBus
    {
        void Publish(string message);
        void Subscribe<T, TH>();
    }
}
