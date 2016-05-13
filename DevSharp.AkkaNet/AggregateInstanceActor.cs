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
    /// <summary>
    /// Actor responsible for the lifecycle of aggregates
    /// Created -> Loading -> Receive | Terminated
    /// </summary>
    public class AggregateInstanceActor :
        UntypedActor,
        IWithUnboundedStash
    {
        private readonly string _identifier;
        private readonly IAggregateClass _aggregateClass;
        private readonly IEventStreamReader _eventReader;
        private object currentState;

        public AggregateInstanceActor(string identifier, IAggregateClass aggregateClass, IEventStreamReader eventReader)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (aggregateClass == null) throw new ArgumentNullException(nameof(aggregateClass));
            if (eventReader == null) throw new ArgumentNullException(nameof(eventReader));

            _identifier = identifier;
            _aggregateClass = aggregateClass;
            _eventReader = eventReader;
        }

        public IStash Stash { get; set; }

        protected override void PreStart()
        {
            base.PreStart();
            Become(OnCreated);
            Self.Tell(new StartLoading());
        }

        #region [ States ]

        private void OnCreated(object message)
        {
            message.Match()
                .With<StartLoading>(OnStartLoadingAggregate)
                .Default(msg => Stash.Stash());
        }

        private void OnLoading(object message)
        {
            message.Match()
                .With<LoadSnapshot>(OnLoadAggregateSnapshot)
                .With<LoadEvent>(OnLoadAggregateEvent)
                .With<LoadEnd>(OnLoadEnd)
                .With<LoadFailed>(OnLoadFailed)
                .With<StartLoading>(Unhandled)
                .Default(msg => Stash.Stash());
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<ProcessCommand>(OnProcessCommand)
                .Default(Unhandled);
        }

        #endregion

        #region [ Handlers ]

        private void OnStartLoadingAggregate(StartLoading message)
        {
            var stream = _eventReader.LoadAggregate(_identifier, withSnapshot: true);
            var self = Self;
            Become(OnLoading);
            IDisposable subscription = null;
            subscription = stream.Subscribe(
                onNext: commit =>
                {
                    if (commit.IsSnapshot)
                        self.Tell(new LoadSnapshot(commit.Payload));
                    else
                        self.Tell(new LoadEvent(commit.Payload));
                },
                onCompleted: () =>
                {
                    self.Tell(LoadEnd.Default);
                    subscription?.Dispose();
                },
                onError: exception =>
                {
                    self.Tell(new LoadFailed(exception));
                    subscription?.Dispose();
                });
        }

        private void OnLoadAggregateSnapshot(LoadSnapshot message)
        {
            currentState = message.Snapshot;
        }

        private void OnLoadAggregateEvent(LoadEvent message)
        {
            var newState = _aggregateClass.ApplyEvent(message.Event, currentState);
            currentState = newState;
        }

        private void OnLoadEnd(LoadEnd message)
        {
            Stash.UnstashAll(envelope => envelope.Message is ProcessCommand);
            Become(OnReceive);
        }

        private void OnLoadFailed(LoadFailed message)
        {
            Self.Tell(Kill.Instance);
        }

        private void OnProcessCommand(ProcessCommand message)
        {
        }

        #endregion

        #region [ Messages ]

        public class StartLoading
        {
            public static readonly StartLoading Default = new StartLoading();
        }

        public class LoadSnapshot
        {
            public LoadSnapshot(object snapshot)
            {
                Snapshot = snapshot;
            }

            public object Snapshot { get; }
        }

        public class LoadEvent
        {
            public LoadEvent(object @event)
            {
                Event = @event;
            }

            public object Event { get; }
        }

        public class LoadEnd
        {
            public static readonly LoadEnd Default = new LoadEnd();
        }

        public class LoadFailed
        {
            public LoadFailed(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }

        public class ProcessCommand
        {
            private readonly ReadOnlyDictionary<string, object> EmptyProperties =
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

            protected ProcessCommand(
                object command,
                IEnumerable<KeyValuePair<string, object>> properties)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));
                Command = command;
                Properties = properties != null
                    ? new ReadOnlyDictionary<string, object>(properties.ToDictionary(p => p.Key, p => p.Value))
                    : EmptyProperties;
            }

            public object Command { get; }
            public IReadOnlyDictionary<string, object> Properties { get; }
        }

        #endregion
    }
}
