using MyNotesApplication.Services.Interfaces;

namespace MyNotesApplication.EventBus
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        public EventBusRabbitMQ() 
        { 

        }

        public void Publish(string message)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T, TH>()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
