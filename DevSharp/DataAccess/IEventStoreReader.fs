namespace DevSharp.DataAccess

open System
open DevSharp
open DevSharp.Messaging

type EventStoreCommit = 
| OnSnapshotCommit of StateType * AggregateVersion * CommandRequest
| OnEventsCommit of EventType list * CommandRequest
| OnCommitsDone
| OnErrorReadingCommits of Exception


[< AbstractClass; Sealed >]
type IEventStoreReader = 
    abstract member Open: unit -> IObservable<EventStoreCommit>


[< AbstractClass; Sealed >]
type IEventStoreReaderFactory =
    abstract member CreateReader : CommandRequest -> IEventStoreReader

