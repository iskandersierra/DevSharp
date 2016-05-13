using System;

namespace DevSharp.Domain
{
    public class FindCommandHandlerResult
    {
        public bool Succees { get; }
        public bool IsObsolete { get; }
        public Type CommandType { get; }
        public ICommandHandler CommandHandler { get; }
        public MessageDescription Description { get; }
    }
}