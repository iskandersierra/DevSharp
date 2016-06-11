module DevSharp.AkkaActors.AggregateActors

open Akka.FSharp
open DevSharp
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.DataAccess

type InstanceActorMessage =
| RequestMessage of InputMessage * Request

let instanceActorFactory (reader: IEventStoreReader) (writer: IEventStoreWriter) aggregateClass aggregateId = 
    let mutable state = init aggregateClass aggregateId

    let handler (mailbox: Actor<InstanceActorMessage>) msg = 
        ()

    handler


let sys = System.create "" (Configuration.load())
let inMemStore = InMemoryEventStore()

let actorFunc = instanceActorFactory inMemStore inMemStore (NopAggregateClass()) "1234"

let actorRef = spawn sys "instance" (actorOf2 actorFunc)

()



