namespace DevSharp.Domain
{
    public class MessageDescription
    {
        public MessageDescription(string ns, string name, string commandName, string version)
        {
            Namespace = ns;
            Name = name;
            CommandName = commandName;
            Version = version;
        }

        public string Namespace { get; }
        public string Name { get; }
        public string CommandName { get; }
        public string Version { get; }
    }
}
