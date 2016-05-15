using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        public const int DoNotStoreSnapshots = 0;
        public const int DefaultStoreSnapshotsEvery = 100;

        private readonly string _identifier;
        private readonly IAggregateClass _aggregateClass;
        private readonly IEventStreamReader _eventReader;
        private readonly IEventStreamWriter _eventWriter;
        private readonly int _storeSnapshotsEveryEvents;
        private object currentState;
        private IDisposable loadSubscription;
        private long version;
        private int unaggregatedEvents;

        public AggregateInstanceActor(
            string identifier, 
            IAggregateClass aggregateClass, 
            IEventStreamReader eventReader, 
            IEventStreamWriter eventWriter,
            int storeSnapshotsEveryEvents = DefaultStoreSnapshotsEvery)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (aggregateClass == null) throw new ArgumentNullException(nameof(aggregateClass));
            if (eventReader == null) throw new ArgumentNullException(nameof(eventReader));
            if (eventWriter == null) throw new ArgumentNullException(nameof(eventWriter));
            if (storeSnapshotsEveryEvents < 0)
                throw new ArgumentOutOfRangeException(nameof(storeSnapshotsEveryEvents));

            _identifier = identifier;
            _aggregateClass = aggregateClass;
            _eventReader = eventReader;
            _eventWriter = eventWriter;
            _storeSnapshotsEveryEvents = storeSnapshotsEveryEvents;
        }

        public IStash Stash { get; set; }

        protected override void PreStart()
        {
            base.PreStart();
            Become(OnCreated);
            Self.Tell(new StartLoading());
            currentState = null;
            loadSubscription = null;
            version = 0;
            unaggregatedEvents = 0;
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
            var stream = _eventReader.LoadEvents(_identifier, withSnapshot: true);
            var self = Self;
            Become(OnLoading);
            loadSubscription = stream.Subscribe(
                onNext: commit =>
                {
                    if (commit.IsSnapshot)
                        self.Tell(new LoadSnapshot(commit.Snapshot, commit.Version));
                    else
                        foreach (var @event in commit.Events)
                            self.Tell(new LoadEvent(@event, commit.Version));
                },
                onCompleted: () =>
                {
                    self.Tell(LoadEnd.Default);
                    loadSubscription?.Dispose();
                },
                onError: exception =>
                {
                    self.Tell(new LoadFailed(exception));
                    loadSubscription?.Dispose();
                });
        }

        private void OnLoadAggregateSnapshot(LoadSnapshot message)
        {
            currentState = message.Snapshot;
            version = message.Version;
            unaggregatedEvents = 0;
        }

        private void OnLoadAggregateEvent(LoadEvent message)
        {
            var newState = _aggregateClass.ApplyEvent(currentState, message.Event);
            currentState = newState;
            version = message.Version;
            unaggregatedEvents++;
        }

        private void OnLoadEnd(LoadEnd message)
        {
            Stash.UnstashAll(envelope => envelope.Message is ProcessCommand);
            Become(OnReceive);
            loadSubscription.Dispose();
            loadSubscription = null;
        }

        private void OnLoadFailed(LoadFailed message)
        {
            Self.Tell(Kill.Instance);
            loadSubscription.Dispose();
            loadSubscription = null;
        }

        private void OnProcessCommand(ProcessCommand message)
        {
            // Execute command

            var events = _aggregateClass.ExecuteCommand(currentState, message.Command).ToArray();
            if (!events.Any())
                events = new object[] { NothingHappened.Default };

            // Apply events

            var newState = events.Aggregate(currentState, _aggregateClass.ApplyEvent);
            currentState = newState;
            var expectedVersion = version;
            version = version += events.Length;
            unaggregatedEvents += events.Length;

            // Send Events to event publisher

            var storeSnapshot = currentState;
            if (unaggregatedEvents < _storeSnapshotsEveryEvents) // do not store snapshots for less than n events
                storeSnapshot = null;
            var ev = new UncommittedAggregateEvents(events, expectedVersion);
            _eventWriter.StoreEventsAsync(_identifier, ev, storeSnapshot);
        }

        #endregion

        #region [ Messages ]

        public class StartLoading
        {
            public static readonly StartLoading Default = new StartLoading();
        }

        public class LoadSnapshot
        {
            public LoadSnapshot(object snapshot, long version)
            {
                Snapshot = snapshot;
                Version = version;
            }

            public object Snapshot { get; }
            public long Version { get; }
        }

        public class LoadEvent
        {
            public LoadEvent(object @event, long version)
            {
                Event = @event;
                Version = version;
            }

            public object Event { get; }
            public long Version { get; }
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

            public ProcessCommand(
                object command,
                IReadOnlyDictionary<string, object> properties)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));
                Command = command;
                Properties = properties ?? EmptyProperties;
            }

            public object Command { get; }
            public IReadOnlyDictionary<string, object> Properties { get; }
        }

        #endregion
    }
}
