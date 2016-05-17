using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka;
using Akka.Actor;
using DevSharp.Domain;
using DevSharp.Messaging;

namespace DevSharp.AkkaNet
{
    public class AggregateInstanceSupervisor : 
        UntypedActor
    {
        private readonly IAggregateClass _aggregateClass;
        private readonly IEventStreamReader _eventReader;
        private readonly IEventStreamWriter _eventWriter;
        private readonly Dictionary<string, AggregateInstanceInfo> _instancesById;
        private readonly int maxInstances;

        public AggregateInstanceSupervisor(IAggregateClass aggregateClass, IEventStreamReader eventReader, IEventStreamWriter eventWriter, int maxInstances = 100)
        {
            if (aggregateClass == null) throw new ArgumentNullException(nameof(aggregateClass));
            if (eventReader == null) throw new ArgumentNullException(nameof(eventReader));
            if (eventWriter == null) throw new ArgumentNullException(nameof(eventWriter));
            if (maxInstances <= 0) throw new ArgumentOutOfRangeException(nameof(maxInstances));
            _aggregateClass = aggregateClass;
            _eventReader = eventReader;
            _eventWriter = eventWriter;
            this.maxInstances = maxInstances;
            _instancesById = new Dictionary<string, AggregateInstanceInfo>(maxInstances);
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return Akka.Actor.SupervisorStrategy.StoppingStrategy;
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<ProcessCommand>(OnProcessCommand)
                .Default(Unhandled);
        }

        private void OnProcessCommand(ProcessCommand message)
        {
            AggregateInstanceInfo info;
            var identifier = message.Identifier;
            if (!_instancesById.TryGetValue(identifier, out info))
            {
                var props = Props.Create(() => new AggregateInstanceActor(identifier, _aggregateClass, _eventReader, _eventWriter, 100));
                var actor = Context.ActorOf(props);

                info = new AggregateInstanceInfo
                {
                    Identifier = identifier,
                    LastAccess = DateTime.UtcNow,
                    Ref = actor,
                };
                _instancesById[identifier] = info;

                var toStop = _instancesById.Count - maxInstances;
                if (toStop > 0)
                {
                    var pairs = _instancesById.OrderBy(p => p.Value.LastAccess).Take(toStop).ToArray();
                    foreach (var pair in pairs)
                    {
                        // TODO: It should be more complex than this. The actor could be dying while a new command is directed to it and a race condition could be produced in the event stream
                        pair.Value.Ref.Tell(PoisonPill.Instance);
                        _instancesById.Remove(identifier);
                    }
                }
            }

            info.Ref.Tell(new AggregateInstanceActor.ProcessCommand(identifier, message.Properties), Sender);
        }

        private class AggregateInstanceInfo
        {
            public string Identifier;
            public IActorRef Ref;
            public DateTime LastAccess;
        }

        public class ProcessCommand
        {
            private readonly ReadOnlyDictionary<string, object> EmptyProperties =
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

            protected ProcessCommand(
                string identifier,
                object command, 
                IReadOnlyDictionary<string, object> properties)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));
                Command = command;
                Identifier = identifier;
                Properties = properties ?? EmptyProperties;
            }

            public string Identifier { get; }
            public object Command { get; }
            public IReadOnlyDictionary<string, object> Properties { get; }
        }
    }
}