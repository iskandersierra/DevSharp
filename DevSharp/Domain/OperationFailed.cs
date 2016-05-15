using System;
using DevSharp.Annotations;

namespace DevSharp.Domain
{
    [AggregateEvent(IsPure = true)]
    public class OperationFailed
    {
        public OperationFailed(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}