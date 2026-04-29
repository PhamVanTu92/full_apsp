namespace BackEndAPI.Service.EventAggregator;

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public void Subscribe<TEvent>(Action<TEvent> action)
    {
        if (!_subscribers.ContainsKey(typeof(TEvent)))
        {
            _subscribers[typeof(TEvent)] = new List<Delegate>();
        }

        _subscribers[typeof(TEvent)].Add(action);
    }


    public void Publish<TEvent>(TEvent eventMessages)
    {
        try
        {
            if (eventMessages != null && _subscribers.ContainsKey(eventMessages.GetType()))
            {
                foreach (var subscriber in _subscribers[eventMessages.GetType()])
                {
                    ((Action<TEvent>)subscriber).Invoke(eventMessages);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // ignored
        }
    }
}