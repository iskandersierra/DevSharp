namespace DevSharp.DataAccess

open System
open System.Linq
open System.Collections.Generic
open DevSharp

type InMemoryEventStore() =

    let allEvents = List<AggregateEventsCommit> (1024)
    let allSnapshots = Dictionary<(TenantId * AggregateType * AggregateId), AggregateSnapshotCommit> (128)
    let lockObj = obj ()

    member this.ReadCommits (observer: IObserver<EventStoreCommit>) request = 
        let tenantId = request.request.tenantId
        let aggregateType = request.aggregateType
        let aggregateId = request.aggregateId
        let key = tenantId, aggregateType, aggregateId
        
        let eventsForRequest minVersion (e: AggregateEventsCommit) = 
            e.firstVersion > minVersion &&
            e.request.aggregate.request.tenantId = tenantId && 
            e.request.aggregate.aggregateType = aggregateType && 
            e.request.aggregate.aggregateId = aggregateId
        
        let getCommits () =
            let (stateFound, state) = allSnapshots.TryGetValue key
            match stateFound with
            | false -> 
                let events = Enumerable.Where (allEvents, eventsForRequest 0)
                None, events
            | true -> 
                let events = Enumerable.Where (allEvents, eventsForRequest state.version)
                Some state, events
            
        let (state, events) = lock lockObj getCommits

        match state with
        | Some s -> observer.OnNext (OnSnapshotCommit s)
        
        events |> Seq.iter (fun e -> observer.OnNext (OnEventsCommit e))


    member this.WriteCommit onSuccess onError request events state version  = 
        let tenantId = request.aggregate.request.tenantId
        let aggregateType = request.aggregate.aggregateType
        let aggregateId = request.aggregate.aggregateId
        let key = tenantId, aggregateType, aggregateId

        let writeCommit () =
            match state with
            | Some s -> 
                let snapshot = { 
                        state = s
                        lastRequest = request
                        version = version
                    }
                allSnapshots.Item(key) <- snapshot

            let commit = { 
                    events = events
                    lastVersion = version
                    firstVersion = version - events.Length
                    request = request
                }
            allEvents.Add commit

        lock lockObj writeCommit

        onSuccess ()

    
    interface IEventStoreReader with
        override this.ReadCommits observer request = 
            this.ReadCommits observer request
    
    interface IEventStoreWriter with
        override this.WriteCommit onSuccess onError request events state version =
            this.WriteCommit onSuccess onError request events state version 

