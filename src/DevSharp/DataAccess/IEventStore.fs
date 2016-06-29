namespace DevSharp.DataAccess

open System
open DevSharp
open System.Threading.Tasks

type EventStoreCommit = 
| OnSnapshotCommit of AggregateSnapshotCommit
| OnEventsCommit of AggregateEventsCommit
with
    static member newVersion: AggregateVersion = 0
and AggregateSnapshotCommit =
    {
        state: StateType
        version: AggregateVersion
    }
and AggregateEventsCommit =
    {
        events: EventType list
        prevVersion: AggregateVersion
        version: AggregateVersion
        request: CommandRequest
    }

type ReadCommitsInput =
    {
        request: AggregateRequest
    }
    with static member create request = 
            { request = request }


type IEventStoreReader =
    abstract member readCommits<'a> : 
        //obs: ObserverFuncs<EventStoreCommit, 'a> -> 
        input: ReadCommitsInput -> 
        //Task<'a>
        IObservable<EventStoreCommit>

type WriteCommitInput =
    {
        request: CommandRequest
        events: EventType list
        state: StateType option
        expectedVersion: AggregateVersion option
    }
    with static member create request events state expectedVersion = 
            { request = request; events = events; state = state; expectedVersion = expectedVersion }

type IEventStoreWriter =
    abstract member writeCommit<'a> : 
        //completion: CompletionFuncs<'a> ->
        input: WriteCommitInput -> 
        //Task<'a>
        IObservable<unit>
