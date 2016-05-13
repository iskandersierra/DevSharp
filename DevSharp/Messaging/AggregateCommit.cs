using System.Collections.Generic;

namespace DevSharp.Messaging
{
    public class AggregateCommit
    {
        public AggregateCommit(bool isSnapshot, object payload, IReadOnlyDictionary<string, object> properties)
        {
            IsSnapshot = isSnapshot;
            Payload = payload;
            Properties = properties;
        }

        public bool IsSnapshot { get; }
        public object Payload { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
    }
}