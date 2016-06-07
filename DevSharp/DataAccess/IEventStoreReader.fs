﻿namespace DevSharp.DataAccess

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
        request: CommandRequest
    }

[< AbstractClass; Sealed >]
type IEventStoreReader =
    abstract member CreateReader : CommandRequest -> IObservable<EventStoreCommit>


