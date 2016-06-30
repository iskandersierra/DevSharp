module DevSharp.AkkaActors.AggregateActors

open Akka
open Akka.Actor
open Akka.FSharp
open System
open System.Reactive
open System.Reactive.Linq
open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.DataAccess
open DevSharp.Validations

type InstanceActorMessage =
| RequestMessage of input: InputMessage
| ApplyEventsMessage of events: EventType list * req: CommandRequest * newBehavior: BehaviorState
with 
    static member requestMessage input = RequestMessage input
    static member applyEventsMessage events request newBehavior = ApplyEventsMessage (events, request, newBehavior)

type InstanceActor(reader: IEventStoreReader, writer: IEventStoreWriter, request, aggregateClass, aggregateId) =
    inherit UntypedActor()
    
    let mutable behavior = init aggregateClass aggregateId
    let mutable stash : Akka.Actor.IStash = null
    
    static let exnToMessage exn = InputMessage.loadError exn |> RequestMessage
    static let doneMessage = InputMessage.loadDone |> RequestMessage

    static let mapCommitToInputMessage =
        function
        | OnSnapshotCommit data -> 
            InputMessage.loadState data.state data.version |> Observable.Return
        | OnEventsCommit data -> 
            data.events 
            |> List.map (fun e -> InputMessage.loadEvent e data.request) 
            |> Observable.ToObservable


    interface IWithUnboundedStash with
        member this.Stash 
            with get () = stash
            and set value = stash <- value

    override this.PreStart() =
        let self = this.Self

        let commitsS = request.aggregate |> ReadCommitsInput.create |> reader.readCommits
        let messagesS = 
            commitsS
                |> Obs.selectMany mapCommitToInputMessage
                |> Obs.select RequestMessage

        do messagesS.SubscribeSafe
            (Observer.Create(
                (fun message -> self <! message), 
                (fun exn -> self <! exnToMessage exn), 
                (fun () -> self <! doneMessage))) 
           |> ignore
        

    override this.OnReceive(msg: obj) =
        let self = this.Self
        let sender = this.Sender
        do 
            match msg with
            | :? InstanceActorMessage as message ->
                match message with
                | RequestMessage input ->
                    let (ReceiveResult (output, newBehavior)) = receive behavior input

                    match output with
                    | SuccessMessage CommandDone ->
                        do sender <! Status.Success ()

                    | SuccessMessage _ -> 
                        do ()

                    | EventsEmitted events ->
                        do WriteCommitInput.create request events None (Some newBehavior.version)
                        |> writer.writeCommit
                        |> Obs.subscribeEnd 
                            (fun exn -> do sender <! Status.Failure exn)
                            (fun () -> do self.Tell(InstanceActorMessage.applyEventsMessage events request newBehavior, sender))
                        |> ignore // ignore the subscription
                                

                    | PostponeMessage ->
                        do stash.Stash()

                    | InvalidCommand validation -> 
                        do sender <! Status.Failure (ValidationException validation)

                    | ExceptionMessage (exn, _) ->
                        do sender <! Status.Failure exn

                    | RejectionMessage _ ->
                        do sender <! Status.Failure (Exception())

                    | _ -> 
                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
                        do failwith (sprintf "Unxpected response type while RequestMessage is processing: %A" output)


                | ApplyEventsMessage (events, request, newBehavior) ->
                    let (ReceiveResult (output, yetNewBehavior)) = receive newBehavior (InputMessage.applyEvents events request)
                    match output with
                    | SuccessMessage _ -> 
                        do behavior <- yetNewBehavior

                    | RejectionMessage _ -> 
                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
                        do failwith (sprintf "Error applying messages to the aggregate while loading")

                    | ExceptionMessage (exn, _) -> 
                        do sender <! Status.Failure exn // TODO: Change Status messages with custom ones
                        do failwith (sprintf "Error applying messages to the aggregate %A" exn)

                    | _ -> 
                        do sender <! Status.Failure (Exception()) // TODO: Change Status messages with custom ones
                        do failwith (sprintf "Unxpected response type while ApplyEventsMessage is processing: %A" output)

                do
                    match behavior.mode with
                    | Receiving -> 
                        do stash.Unstash()
                    | _ -> 
                        do ()

            | _ -> 
                do this.Unhandled()

