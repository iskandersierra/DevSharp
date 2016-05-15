using DevSharp.Annotations;

namespace DevSharp.Domain
{
    [AggregateEvent(IsPure = true)]
    public class OperationCancelled
    {
        public static readonly OperationCancelled Default = new OperationCancelled();
    }
}