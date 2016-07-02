module DevSharp.AkkaActors.AggregateActors

open System
open System.Reactive
open System.Reactive.Linq
open Akka
open Akka.Actor
open Akka.FSharp
open NLog;

open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.DataAccess
open DevSharp.Validations

type InstanceActorInput =
    {
        command: CommandType
        expectedVersion: AggregateVersion option
        request: CommandRequest
    }

type InstanceActor(reader: IEventStoreReader, writer: IEventStoreWriter, aggregateRequest, aggregateClass) =
    inherit UntypedActor()

    static let log = LogManager.GetLogger((typedefof<InstanceActor>).FullName)

    let mutable actorState = initialBehavior aggregateClass aggregateRequest.aggregateId
    let mutable stash : Akka.Actor.IStash = null
    
    let exnToMessage exn = InputMessage.loadError exn
    let doneMessage = InputMessage.loadDone
    
    do log.Trace ("Created")

    override this.PreStart() =
        let self = this.Self
        do log.Trace ("PreStart")

        let messagesS = readCurrentEvents reader aggregateRequest            

        do messagesS.SubscribeSafe
            (Observer.Create(
                (fun message -> self <! message), 
                (fun exn -> self <! exnToMessage exn), 
                (fun () -> self <! doneMessage))) 
           |> ignore

    override this.OnReceive(msg: obj) =
        let self = this.Self
        let sender = this.Sender

        let actions = {
            new IAggregateActorActions with
                member __.success () = 
                    do sender <! Status.Success ()
                
                member __.reject result = 
                    do sender <! Status.Failure (ValidationException result)
                
                member __.emit events request writing = 
                    do writing
                    |> Obs.subscribeEnd 
                        (fun exn -> do sender <! Status.Failure exn)
                        (fun () -> do self.Tell(InputMessage.applyEvents events request, sender))
                    |> ignore // ignore the subscription
                
                member __.error kind msg = 
                    do sender <! Status.Failure (Exception msg)
                
                member __.postpone () = 
                    do stash.Stash()
                
                member __.recover () = 
                    do stash.UnstashAll()
        }

        do match msg with
            | :? InputMessage as message ->
                do actorState <- receive writer actions actorState message
                // if Corrupted terminate actor?
            
            | :? InstanceActorInput as input ->
                do actorState <- receive writer actions actorState (InputMessage.receiveCommand input.command input.expectedVersion input.request)
            
            | _ -> 
                do this.Unhandled()
        do ()
        (*
        do 
            match msg with
            | :? InstanceActorMessage as message ->
                match message with
                | RequestMessage input ->
                    let (ReceiveResult (output, newBehavior)) = receive behavior input

                    match output with
                    | SuccessMessage successType -> 
                        successType 
                        |> function 
                        | CommandDone -> do sender <! Status.Success () 
                        | _ -> do ()

                    | EventsEmitted (events, request) ->
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
                do log.Trace (sprintf "OnReceive %A from %O stayed as %A" msg sender behavior)

            | _ -> 
                do this.Unhandled()

        *)
    interface IWithUnboundedStash with
        member this.Stash 
            with get () = stash
            and set value = stash <- value

