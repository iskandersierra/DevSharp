using DevSharp.Annotations;

namespace DevSharp.Domain
{
    [AggregateEvent(IsPure = true)]
    public class NothingHappened
    {
        public static readonly NothingHappened Default = new NothingHappened();
    }
}
