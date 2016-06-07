namespace DevSharp.DataAccess

open System
open DevSharp
open DevSharp.Messaging

type EventStoreCommit = 
| OnSnapshotCommit of StateType * AggregateVersion * Request
| OnEventsCommit of EventType list * Request
| OnCommitsDone
| OnErrorReadingCommits of Exception


[< AbstractClass; Sealed >]
type IEventStoreReader = 
    abstract member Open: unit -> IObservable<EventStoreCommit>


[< AbstractClass; Sealed >]
type IEventStoreReaderFactory =
    abstract member CreateReader : Request -> IEventStoreReader

