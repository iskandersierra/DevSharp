namespace DevSharp.Domain
{
    public interface IAggregateClass
    {
        object ApplyEvent(object @event, object currentState);
    }
}
