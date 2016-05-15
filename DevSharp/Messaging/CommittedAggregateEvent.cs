using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevSharp.Messaging
{
    public class CommittedAggregateEvent
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyProperties = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        public CommittedAggregateEvent(object snapshot, long version, IEnumerable<KeyValuePair<string, object>> properties = null)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            IsSnapshot = true;
            Snapshot = snapshot;
            Properties = properties == null
                ? EmptyProperties
                : new ReadOnlyDictionary<string, object>(properties.ToDictionary(p => p.Key, p => p.Value));
            Version = version;
        }

        public CommittedAggregateEvent(IEnumerable<object> events, long version, IEnumerable<KeyValuePair<string, object>> properties = null)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            IsSnapshot = false;
            Events = new ReadOnlyCollection<object>(events.ToArray());
            Properties = properties == null
                ? EmptyProperties
                : new ReadOnlyDictionary<string, object>(properties.ToDictionary(p => p.Key, p => p.Value));
            Version = version;
        }

        public bool IsSnapshot { get; }
        public object Snapshot { get; }
        public IReadOnlyCollection<object> Events { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public long Version { get; }
    }
}