using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka;
using Akka.Actor;
using Akka.Routing;
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

    public class AggregateClassSupervisor :
        UntypedActor
    {
        private readonly IAggregateClass _aggregateClass;
        private readonly IEventStreamReader _eventReader;
        private readonly IEventStreamWriter _eventWriter;
        private readonly int _maxInstances = 100;
        private IActorRef validators;
        private IActorRef executor;

        public AggregateClassSupervisor(IAggregateClass aggregateClass, IEventStreamReader eventReader, IEventStreamWriter eventWriter, int maxInstances = 100)
        {
            if (aggregateClass == null) throw new ArgumentNullException(nameof(aggregateClass));
            if (eventReader == null) throw new ArgumentNullException(nameof(eventReader));
            if (eventWriter == null) throw new ArgumentNullException(nameof(eventWriter));
            if (maxInstances <= 0) throw new ArgumentOutOfRangeException(nameof(maxInstances));
            _aggregateClass = aggregateClass;
            _eventReader = eventReader;
            _eventWriter = eventWriter;
            _maxInstances = maxInstances;
        }

        protected override void PreStart()
        {
            base.PreStart();

            var validatorProps = Props.Create(() => new AggregateValidatorActor(_aggregateClass))
                .WithRouter(new SmallestMailboxPool(10)) // TODO: Make configurable
                .WithSupervisorStrategy(Akka.Actor.SupervisorStrategy.StoppingStrategy);

            validators = Context.ActorOf(validatorProps);

            var executorProps = Props.Create(() => new AggregateInstanceSupervisor(_aggregateClass, _eventReader, _eventWriter, _maxInstances));

            executor = Context.ActorOf(executorProps);
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<ProcessCommand>(OnProcessCommand)
                .Default(Unhandled);
        }

        private void OnProcessCommand(ProcessCommand message)
        {
            var validateMessage = new AggregateValidatorActor.ValidateCommand(message.Command, message.Properties);
            var validateTask = validators.Ask<AggregateValidatorActor.ValidatedCommand>(validateMessage);
            var self = Self;
            var sender = Sender;
            validateTask.ContinueWith(vt =>
            {
                if (vt.IsFaulted)
                    sender.Tell(new OperationFailed(vt.Exception), ActorRefs.NoSender);
                else if (vt.IsCanceled)
                    sender.Tell(new OperationCancelled(), ActorRefs.NoSender);
                else if (!vt.Result.Result.IsValid)
                    sender.Tell(new ValidationFailed(vt.Result.Result), ActorRefs.NoSender);
                else
                    executor.Tell(new AggregateInstanceActor.ProcessCommand(message.Command, message.Properties), sender);
            });
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