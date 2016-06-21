namespace DevSharp.DataAccess

open System
open System.Linq
open System.Collections.Generic
open DevSharp
open System.Threading.Tasks

type InMemoryEventStore() =

    let allEvents = List<AggregateEventsCommit> (1024)
    let allSnapshots = Dictionary<(TenantId * AggregateType * AggregateId), AggregateSnapshotCommit> (128)
    let lockObj = obj ()

    member __.ReadCommits 
        (onNext: (EventStoreCommit -> unit))
        (onCompleted: (unit -> 'a))
        (onError: (Exception -> 'a))
        request = 
        let tenantId = request.request.tenantId
        let aggregateType = request.aggregateType
        let aggregateId = request.aggregateId
        let key = tenantId, aggregateType, aggregateId
        
        let eventsForRequest (minVersion) (e: AggregateEventsCommit) = 
            e.prevVersion >= minVersion &&
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

        let task : Async<'a> = 
            async {
                let (state, events) = lock lockObj getCommits
                do match state with
                   | Some s -> onNext (OnSnapshotCommit s)
                   | _ -> do ()
                do events |> Seq.iter (fun e -> onNext (OnEventsCommit e))
                return onCompleted ()
            }
        Async.StartAsTask task


    member __.WriteCommit 
        (onSuccess: (unit -> 'a)) 
        (onError: (Exception -> 'a)) 
        request events state version = 
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
            | _ -> do ()

            let commit = { 
                    events = events
                    lastVersion = version
                    prevVersion = version - events.Length
                    request = request
                }
            do allEvents.Add commit

        let task : Async<'a> = 
            async {
                do lock lockObj writeCommit
                do! Async.Sleep 10
                return onSuccess ()
            }
        Async.StartAsTask task

    member __.getAllEvents () =
        lock lockObj (fun () -> allEvents |> Seq.toList)

    member __.getAllSnapshots () =
        lock lockObj (fun () -> allSnapshots |> Seq.map (fun pair -> pair.Key, pair.Value) |> Map.ofSeq)
    
    interface IEventStoreReader with
        override this.ReadCommits onNext onCompleted onError request = 
            this.ReadCommits onNext onCompleted onError request
    
    interface IEventStoreWriter with
        override this.WriteCommit onSuccess onError request events state version =
            this.WriteCommit onSuccess onError request events state version 

