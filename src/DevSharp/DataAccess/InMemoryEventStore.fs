namespace DevSharp.DataAccess

open System
open System.Linq
open System.Collections.Generic
open DevSharp
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Disposables

//type Observable = System.Reactive.Linq.Observable

type AggregateKey = TenantId * AggregateType * AggregateId
type PersistedAggregate =
    {
        key: AggregateKey
        version: AggregateVersion
        events: AggregateEventsCommit list
        snapshot: AggregateSnapshotCommit option
    }

type InMemoryEventStore() =

    let aggregates = Dictionary<AggregateKey, PersistedAggregate> (16)
    let lockObj = obj ()

    member __.readCommits (input: ReadCommitsInput) = 
        let tenantId = input.request.request.tenantId
        let aggregateType = input.request.aggregateType
        let aggregateId = input.request.aggregateId
        let key = tenantId, aggregateType, aggregateId
        
        let eventsForRequest (minVersion) (e: AggregateEventsCommit) = 
            e.prevVersion >= minVersion &&
            e.request.aggregate.request.tenantId = tenantId && 
            e.request.aggregate.aggregateType = aggregateType && 
            e.request.aggregate.aggregateId = aggregateId
        
        let getCommits () =
            let (found, aggregate) = aggregates.TryGetValue key
            match found with
            | false -> None, Seq.empty
            | true -> 
                match aggregate.snapshot with
                | None ->
                    let events = aggregate.events 
                                |> List.toSeq
                    None, events
                | Some state ->
                    let events = aggregate.events 
                                |> Seq.filter (eventsForRequest state.version)
                                |> Seq.toList
                                |> List.toSeq
                    Some state, events

        let (state, events) = lock lockObj getCommits

        let stateS = 
            match state with
            | Some s -> Observable.Return(OnSnapshotCommit s)
            | _ -> Observable.Empty()
        let eventsS = events.ToObservable().Select OnEventsCommit
        
        stateS.Concat(eventsS)


    member __.writeCommit (input: WriteCommitInput) = 
        let tenantId = input.request.aggregate.request.tenantId
        let aggregateType = input.request.aggregate.aggregateType
        let aggregateId = input.request.aggregate.aggregateId
        let key = tenantId, aggregateType, aggregateId

        let writeCommit () =
            let found, foundAggregate = aggregates.TryGetValue key

            let aggregate = 
                match found with 
                | false -> 
                    {
                        key = key
                        version = 0
                        events = []
                        snapshot = None
                    }
                | true -> foundAggregate

            match input.expectedVersion.IsSome && input.expectedVersion.Value <> aggregate.version with
            | true -> 
                AFailure "Unexpected aggregate version while writing commit"

            | false -> 
                let commit = { 
                        events = input.events
                        version = aggregate.version + input.events.Length
                        prevVersion = aggregate.version
                        request = input.request
                    }

                let newEvents = aggregate.events @ [ commit ]

                let newVersion = aggregate.version + (input.events |> List.length)

                let newSnapshot = 
                    match input.state with
                    | None -> aggregate.snapshot
                    | Some state -> 
                        Some {
                            state = state
                            version = newVersion
                        }

                let newAggregate = 
                    {
                        aggregate with
                            version = newVersion
                            events = newEvents
                            snapshot = newSnapshot
                    }

                do aggregates.Item(key) <- newAggregate

                ASuccess ()

        match lock lockObj writeCommit with
        | ASuccess _ -> Observable.Empty()
        | AFailure message -> Observable.Throw (Exception(message))

    
    interface IEventStoreReader with
        override this.readCommits input = 
            this.readCommits input
    
    interface IEventStoreWriter with
        override this.writeCommit input =
            this.writeCommit input

