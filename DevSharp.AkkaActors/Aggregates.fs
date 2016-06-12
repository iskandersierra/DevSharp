module DevSharp.AkkaActors.AggregateActors

open Akka
open Akka.Actor
open Akka.FSharp
open System
open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.DataAccess
open DevSharp.Validations

type InstanceActorMessage =
| RequestMessage of input: InputMessage * req: CommandRequest

let instanceActorFactory (reader: IEventStoreReader) (writer: IEventStoreWriter) aggregateClass aggregateId = 
    let mutable state = init aggregateClass aggregateId

    //reader.ReadCommits observer request

    let handler (mailbox: Actor<InstanceActorMessage>) msg = 
        mailbox.Self <! msg
        ()

    handler


type InstanceActor(reader: IEventStoreReader, writer: IEventStoreWriter, request, aggregateClass, aggregateId) =
    inherit UntypedActor()
    let mutable behavior = init aggregateClass aggregateId
    let mutable stash : Akka.Actor.IStash = null

    interface IWithUnboundedStash with
        member this.Stash 
            with get () = stash
            and set value = stash <- value

    override this.PreStart() =
        let self = this.Self
        let observer = 
            { 
                new IObserver<EventStoreCommit> with
                    member obs.OnNext commit = 
                        match commit with
                        | OnSnapshotCommit data ->
                            self <! RequestMessage (LoadState (data.state, data.version), data.lastRequest)
                        | OnEventsCommit data ->
                            data.events
                            |> List.map ( fun e -> RequestMessage (LoadEvent e, data.request) ) 
                            |> List.iter self.Tell
                        ()
                    member obs.OnError exn = 
                        self <! RequestMessage (LoadError exn, request)
                    member obs.OnCompleted () = 
                        self <! RequestMessage (LoadDone, request)
            }

        reader.ReadCommits observer request.aggregate
        ()

    override this.OnReceive(msg: obj) =
        let self = this.Self
        let sender = this.Sender
        match msg with
        | :? InstanceActorMessage as message ->
            match message with
            | RequestMessage (input, request) ->
                let (ReceiveResult (output, newBehavior)) = receive behavior input request
                behavior <- newBehavior

                match output with
                | CommandDone ->
                    sender <! Status.Success ()

                | EventsEmitted events ->
                    writer.WriteCommit
                        (fun () -> self.Tell(RequestMessage (ApplyEvents events, request), sender))
                        (fun exn -> sender <! Status.Failure exn )
                        request
                        events
                        None
                        newBehavior.version

                | PostponeMessage ->
                    stash.Stash()

                | InvalidCommand validation -> 
                    sender <! Status.Failure (ValidationException validation)

                | ValidateFailed exn 
                | ActFailed      exn 
                | LoadingFailed  exn 
                | ApplyFailed    exn ->
                    sender <! Status.Failure exn

                | UnexpectedVersion ->
                    sender <! Status.Failure (Exception())

                | MessageRejected ->
                    sender <! Status.Failure (Exception())

                | MessageAccepted -> ()

            match behavior.mode with
            | Receiving -> 
                stash.Unstash()
            | _ -> ()

        | _ -> 
            this.Unhandled()



let sys        = System.create "" (Configuration.load())
let inMemStore = InMemoryEventStore()

let instance   = sys.ActorOf(Props.Create<InstanceActor>())

let actorFunc  = instanceActorFactory inMemStore inMemStore (NopAggregateClass()) "1234"

let actorRef   = spawn sys "instance" (actorOf2 actorFunc)

()



