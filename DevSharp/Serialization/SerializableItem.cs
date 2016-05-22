using System.Collections.Generic;

namespace DevSharp.Serialization
{
    public class SerializableItem
    {
        public string Namespace { get; }
        public string ItemType { get; }
        public string Name { get; }
        public string Version { get; }
        public IDictionary<string, object> Item { get; }
        public IDictionary<string, object> Metadata { get; }
    }
}
