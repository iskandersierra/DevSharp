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
| RequestMessage of input: InputMessage
| ApplyEventsMessage of events: EventType list * req: CommandRequest * newBehavior: BehaviorState
//
//type InstanceActor(reader: IEventStoreReader, writer: IEventStoreWriter, request, aggregateClass, aggregateId) =
//    inherit UntypedActor()
//    let mutable behavior = init aggregateClass aggregateId
//    let mutable stash : Akka.Actor.IStash = null
//
//    interface IWithUnboundedStash with
//        member this.Stash 
//            with get () = stash
//            and set value = stash <- value
//
//    override this.PreStart() =
//        let self = this.Self
//        let onNext commit = 
//            match commit with
//            | OnSnapshotCommit data ->
//                do self <! RequestMessage (LoadState (data.state, data.version))
//            | OnEventsCommit data ->
//                do [for e in data.events -> RequestMessage (LoadEvent (e, data.request))]
//                |> List.iter self.Tell
//        let onError exn = 
//            do self <! RequestMessage (LoadError exn)
//        let onCompleted () = 
//            do self <! RequestMessage LoadDone
//
//        do reader.readCommits 
//                (observer onNext onCompleted onError) 
//                (ReadCommitsInput.create request.aggregate)
//           |> ignore
//        
//
//    override this.OnReceive(msg: obj) =
//        let self = this.Self
//        let sender = this.Sender
//        do 
//            match msg with
//            | :? InstanceActorMessage as message ->
//                match message with
//                | RequestMessage input ->
//                    let (ReceiveResult (output, newBehavior)) = receive behavior input
//
//                    match output with
//                    | CommandDone ->
//                        do sender <! Status.Success ()
//
//                    | EventsEmitted events ->
//                        let onCommitted () = 
//                            do self.Tell(ApplyEventsMessage (events, request, newBehavior), sender)
//                        let onError exn = 
//                            do sender <! Status.Failure exn
//                        let task = 
//                            writer.writeCommit 
//                                (completion onCommitted onError) 
//                                (WriteCommitInput.create request events None newBehavior.version)
//                        do task |> ignore
//                                
//
//                    | PostponeMessage ->
//                        do stash.Stash()
//
//                    | InvalidCommand validation -> 
//                        do sender <! Status.Failure (ValidationException validation)
//
//                    | ValidateFailed exn 
//                    | ActFailed      exn 
//                    | LoadingFailed  exn ->
//                        do sender <! Status.Failure exn
//
//                    | UnexpectedVersion 
//                    | MessageRejected ->
//                        do sender <! Status.Failure (Exception())
//
//                    | MessageAccepted -> 
//                        do ()
//
//                    | _ -> 
//                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
//                        do failwith (sprintf "Unxpected response type while RequestMessage is processing: %A" output)
//
//
//                | ApplyEventsMessage (events, request, newBehavior) ->
//                    let (ReceiveResult (output, yetNewBehavior)) = receive newBehavior (ApplyEvents (events, request))
//                    match output with
//                    | MessageAccepted -> 
//                        do behavior <- yetNewBehavior
//
//                    | MessageRejected -> 
//                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
//                        do failwith (sprintf "Error applying messages to the aggregate while loading")
//
//                    | ApplyFailed exn -> 
//                        do sender <! Status.Failure exn // TODO: Change Status messages with custom ones
//                        do failwith (sprintf "Error applying messages to the aggregate %A" exn)
//
//                    | _ -> 
//                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
//                        do failwith (sprintf "Unxpected response type while ApplyEventsMessage is processing: %A" output)
//
//                do
//                    match behavior.mode with
//                    | Receiving -> 
//                        do stash.Unstash()
//                    | _ -> 
//                        do ()
//
//            | _ -> 
//                do this.Unhandled()
//
