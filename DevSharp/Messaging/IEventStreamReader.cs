using System;

namespace DevSharp.Messaging
{
    public interface IEventStreamReader
    {
        IObservable<CommittedAggregateEvent> LoadEvents(string identifier, bool withSnapshot);
    }
}
