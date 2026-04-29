namespace BackEndAPI.Service.EventAggregator;

public interface IEventAggregator
{
    void Subscribe<TEvent>(Action<TEvent> action);
    void Publish<TEvent>(TEvent eventMessages);
}