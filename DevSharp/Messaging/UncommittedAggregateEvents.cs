using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevSharp.Messaging
{
    public class UncommittedAggregateEvents
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyProperties = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        public UncommittedAggregateEvents(IEnumerable<object> events, long expectedVersion, IEnumerable<KeyValuePair<string, object>> properties = null)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            Events = new ReadOnlyCollection<object>(events.ToArray());
            Properties = properties == null
                ? EmptyProperties
                : new ReadOnlyDictionary<string, object>(properties.ToDictionary(p => p.Key, p => p.Value));
            ExpectedVersion = expectedVersion;
        }

        public IReadOnlyCollection<object> Events { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public long ExpectedVersion { get; }
    }
}