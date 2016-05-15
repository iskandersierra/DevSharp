using DevSharp.Annotations;

namespace DevSharp.Messaging
{
    [AggregateEvent(IsPure = true)]
    public class NothingHappened
    {
        public static readonly NothingHappened Default = new NothingHappened();
    }
}
