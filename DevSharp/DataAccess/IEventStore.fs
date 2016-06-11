namespace DevSharp.DataAccess

open System
open DevSharp

type EventStoreCommit = 
| OnSnapshotCommit of AggregateSnapshotCommit
| OnEventsCommit of AggregateEventsCommit
and AggregateSnapshotCommit =
    {
        state: StateType
        version: AggregateVersion
        lastRequest: CommandRequest
    }
and AggregateEventsCommit =
    {
        events: EventType list
        firstVersion: AggregateVersion
        lastVersion: AggregateVersion
        request: CommandRequest
    }

type IEventStoreReader =
    abstract member ReadCommits : IObserver<EventStoreCommit> -> AggregateRequest -> unit


type IEventStoreWriter =
    abstract member WriteCommit : (unit -> 'a) -> (Exception -> 'a) -> CommandRequest -> EventType list -> StateType option -> AggregateVersion -> 'a
