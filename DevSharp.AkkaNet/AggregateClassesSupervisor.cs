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
    public class AggregateClassesSupervisor : 
        UntypedActor
    {
        private readonly IEventStreamReader _eventReader;
        private readonly IEventStreamWriter _eventWriter;
        private readonly Dictionary<string, IAggregateClass> _aggregateClasses;
        private Dictionary<string, IActorRef> _classSupervisors;

        public AggregateClassesSupervisor(IEventStreamReader eventReader, IEventStreamWriter eventWriter, IEnumerable<KeyValuePair<string, IAggregateClass>> aggregateClasses)
        {
            if (eventReader == null) throw new ArgumentNullException(nameof(eventReader));
            if (eventWriter == null) throw new ArgumentNullException(nameof(eventWriter));
            if (aggregateClasses == null) throw new ArgumentNullException(nameof(aggregateClasses));
            _eventReader = eventReader;
            _eventWriter = eventWriter;
            _aggregateClasses = aggregateClasses
                .ToDictionary(p => p.Key, p => p.Value);
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(Decider.From(Directive.Escalate));
        }

        protected override void PreStart()
        {
            base.PreStart();

            // TODO: This is wrong!!! Should not pass services through Props, only keys and use DI to resolve the services in the respective cluster node
            _classSupervisors = _aggregateClasses
                .ToDictionary(p => p.Key,
                    p => Context.ActorOf(Props.Create(() => new AggregateClassSupervisor(p.Value, _eventReader, _eventWriter, 100))));
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<ProcessCommand>(OnProcessCommand)
                .Default(Unhandled);

        }

        private void OnProcessCommand(ProcessCommand message)
        {
            IActorRef actorRef;
            if (!_classSupervisors.TryGetValue(message.AggregateName, out actorRef) || actorRef == null)
                Sender.Tell(new Status.Failure(new KeyNotFoundException($"Aggregate {message.AggregateName} is unknown")));

            var msg = new AggregateClassSupervisor.ProcessCommand(message.Identifier, message.Command, message.Properties);

            actorRef.Tell(msg, Sender);
        }

        public class ProcessCommand
        {
            private readonly ReadOnlyDictionary<string, object> EmptyProperties =
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

            protected ProcessCommand(
                string aggregateName,
                string identifier,
                object command, 
                IReadOnlyDictionary<string, object> properties)
            {
                if (aggregateName == null) throw new ArgumentNullException(nameof(aggregateName));
                if (command == null) throw new ArgumentNullException(nameof(command));
                Command = command;
                AggregateName = aggregateName;
                Identifier = identifier;
                Properties = properties ?? EmptyProperties;
            }

            public string AggregateName { get; }
            public string Identifier { get; }
            public object Command { get; }
            public IReadOnlyDictionary<string, object> Properties { get; }
        }
    }
}