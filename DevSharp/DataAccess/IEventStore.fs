namespace DevSharp.DataAccess

open System
open DevSharp
open System.Threading.Tasks

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
    abstract member ReadCommits<'a> : 
        onNext: (EventStoreCommit -> unit) -> 
        onCompleted: (unit -> 'a) -> 
        onError: (Exception -> 'a) -> 
        request: AggregateRequest -> 
        Task<'a>


type IEventStoreWriter =
    abstract member WriteCommit<'a> : 
        onSuccess: (unit -> 'a) -> 
        onError: (Exception -> 'a) -> 
        request: CommandRequest -> 
        events: EventType list -> 
        state: StateType option -> 
        version: AggregateVersion -> 
        Task<'a>
