using System;

namespace DevSharp.Domain
{
    public class CommandPrecondition<TCommand>
    {
        public CommandPrecondition(TCommand command)
        {
            if (ReferenceEquals(command, null)) throw new ArgumentNullException(nameof(command));
            Command = command;
        }

        public TCommand Command { get; }
    }

    public class CommandPrecondition<TCommand, TState>
    {
        public CommandPrecondition(TCommand command, TState state)
        {
            if (ReferenceEquals(command, null)) throw new ArgumentNullException(nameof(command));
            if (ReferenceEquals(state, null)) throw new ArgumentNullException(nameof(state));
            Command = command;
            State = state;
        }

        public TCommand Command { get; }
        public TState State { get; }
    }
}
