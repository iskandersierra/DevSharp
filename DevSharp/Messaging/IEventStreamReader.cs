using System;

namespace DevSharp.Messaging
{
    public interface IEventStreamReader
    {
        IObservable<AggregateCommit> LoadAggregate(string identifier, bool withSnapshot);
    }
}
