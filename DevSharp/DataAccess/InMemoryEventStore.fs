namespace DevSharp.DataAccess

open System
open System.Linq
open System.Collections.Generic
open DevSharp

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

    member __.readCommits (obs: ObserverFuncs<EventStoreCommit, 'a>) (input: ReadCommitsInput) = 
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
                    //let events =  Enumerable.Where (aggregate.events, eventsForRequest state.version)
                    let events = aggregate.events 
                                |> Seq.filter (eventsForRequest state.version)
                                |> Seq.toList
                                |> List.toSeq
                    Some state, events

        let task : Async<'a> = 
            async {
                let (state, events) = lock lockObj getCommits
                do match state with
                   | Some s -> obs.onNext (OnSnapshotCommit s)
                   | _ -> do ()
                do events |> Seq.iter (fun e -> obs.onNext (OnEventsCommit e))
                return obs.onCompleted ()
            }
        Async.StartAsTask task


    member __.writeCommit (completion: CompletionFuncs<'a>) (input: WriteCommitInput) = 
        let tenantId = input.request.aggregate.request.tenantId
        let aggregateType = input.request.aggregate.aggregateType
        let aggregateId = input.request.aggregate.aggregateId
        let key = tenantId, aggregateType, aggregateId

        let writeCommit (completion: CompletionFuncs<'a>) () =
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

            match input.expectedVersion = aggregate.version with
            | false -> 
                completion.onError (Exception("Unexpected aggregate version while writing commit"))

            | true -> 
                let commit = { 
                        events = input.events
                        version = input.expectedVersion
                        prevVersion = input.expectedVersion - input.events.Length
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

                completion.onCompleted ()

        let task : Async<'a> = 
            async {
                return lock lockObj (writeCommit completion)
            }
        Async.StartAsTask task

    member __.getAllAggregates () =
        let toTuple (pair: KeyValuePair<'a, 'b>) = pair.Key, pair.Value
        let toMap () = aggregates |> Seq.map toTuple |> Map.ofSeq
        lock lockObj toMap
    
    interface IEventStoreReader with
        override this.readCommits obs input = 
            this.readCommits obs input
    
    interface IEventStoreWriter with
        override this.writeCommit completion input =
            this.writeCommit completion input

