using DevSharp.Annotations;

namespace DevSharp.Messaging
{
    [AggregateEvent(IsPure = true)]
    public class OperationCancelled
    {
        public static readonly OperationCancelled Default = new OperationCancelled();
    }
}