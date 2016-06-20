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
open System.Threading.Tasks

type InstanceActorMessage =
| RequestMessage of input: InputMessage * req: CommandRequest

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
        let onNext commit = 
            match commit with
            | OnSnapshotCommit data ->
                do self <! RequestMessage (LoadState (data.state, data.version), data.lastRequest)
            | OnEventsCommit data ->
                do [for e in data.events -> RequestMessage (LoadEvent e, data.request)]
                |> List.iter self.Tell
        let onError exn = 
            do self <! RequestMessage (LoadError exn, request)
        let onCompleted () = 
            do self <! RequestMessage (LoadDone, request)

        reader.ReadCommits onNext onCompleted onError request.aggregate
        |> ignore
        

    override this.OnReceive(msg: obj) =
        let self = this.Self
        let sender = this.Sender
        do 
            match msg with
            | :? InstanceActorMessage as message ->
                match message with
                | RequestMessage (input, request) ->
                    let (ReceiveResult (output, newBehavior)) = receive behavior input request
                    do behavior <- newBehavior

                    do
                        match output with
                        | CommandDone ->
                            do sender <! Status.Success ()

                        | EventsEmitted events ->
                            let onCommitted () = self.Tell(RequestMessage (ApplyEvents events, request), sender)
                            let onError exn = sender <! Status.Failure exn
                            do writer.WriteCommit onCommitted onError request events None newBehavior.version

                        | PostponeMessage ->
                            do stash.Stash()

                        | InvalidCommand validation -> 
                            do sender <! Status.Failure (ValidationException validation)

                        | ValidateFailed exn 
                        | ActFailed      exn 
                        | LoadingFailed  exn 
                        | ApplyFailed    exn ->
                            do sender <! Status.Failure exn

                        | UnexpectedVersion 
                        | MessageRejected ->
                            do sender <! Status.Failure (Exception())

                        | MessageAccepted -> 
                            do ()

                do
                    match behavior.mode with
                    | Receiving -> 
                        do stash.Unstash()
                    | _ -> 
                        do ()

            | _ -> 
                do this.Unhandled()

