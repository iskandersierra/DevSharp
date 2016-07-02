module DevSharp.Domain.Aggregates.AggregateBehavior

open System
open System.Reactive
open System.Reactive.Linq
open DevSharp
open DevSharp.DataAccess
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils

type InputMessage =
    | LoadingMessage   of LoadingMessage
    | ReceivingMessage of ReceivingMessage
    | EmittingMessage  of EmittingMessage
    with
        static member loadState state version                        = LoadingMessage (LoadState (state, version))
        static member loadEvent event request                        = LoadingMessage (LoadEvent (event, request))
        static member loadError exn                                  = LoadingMessage (LoadError exn)
        static member loadDone                                       = LoadingMessage (LoadDone)
        static member receiveCommand command expectedVersion request = ReceivingMessage (ReceiveCommand (command, expectedVersion, request))
        static member applyEvents events request                     = EmittingMessage (ApplyEvents (events, request))
and LoadingMessage   =
    | LoadState          of StateType * AggregateVersion
    | LoadEvent          of EventType * CommandRequest
    | LoadError          of Exception
    | LoadDone
and ReceivingMessage =
    | ReceiveCommand     of CommandType * AggregateVersion option * CommandRequest
and EmittingMessage  =
    | ApplyEvents        of EventType list * CommandRequest


type ErrorResult =
    | UnexpectedVersion
    | MessageRejected
    | ValidateFailed
    | ActFailed
    | ApplyFailed
    | LoadingFailed


type Mode =
    | Loading
    | Receiving
    | Emitting
    | Corrupted

type BehaviorState =
    {
        mode: Mode;
        version: AggregateVersion
        id: AggregateId
        state: StateType
        class': IAggregateClass
    }


type IAggregateActorActions =
    abstract member success   : unit                    -> unit
    abstract member reject    : ValidationResult        -> unit
    abstract member emit      : EventType list -> CommandRequest -> IObservable<unit> -> unit
    abstract member error     : ErrorResult -> string   -> unit
    abstract member postpone  : unit                    -> unit
    abstract member recover   : unit                    -> unit

(*     Internal functions      *)

let mapCommitToInputMessage =
    function
    | OnSnapshotCommit data -> 
        InputMessage.loadState data.state data.version |> Observable.Return
    | OnEventsCommit data -> 
        data.events 
        |> List.map (fun e -> InputMessage.loadEvent e data.request) 
        |> Observable.ToObservable

let validateCommand onValid onInvalid onError behavior command request =
    try
        let result = behavior.class'.validate command request
        match result.isValid with
        | true -> onValid ()
        | false -> onInvalid result
    with 
    | ex -> onError ex

let applyEvent onApplied onError behavior event request =
    try
        let newState = behavior.class'.apply event behavior.state request
        let next = { 
            behavior with  
                version = behavior.version + 1
                state = newState;
        } 
        onApplied next
    with
    | ex -> onError ex

let rec applyEvents onApplied onError behavior events request =
    match events with
    | [] -> onApplied behavior
    | event :: tail ->
        let continuation next = applyEvents onApplied onError next tail request
        applyEvent continuation onError behavior event request

let actOnCommand onActed onNotActed onError bahavior state command request =
    try
        let maybeEvents = bahavior.class'.act command state request
        match maybeEvents with
        | Some events -> onActed ( Seq.toList events )
        | None -> onNotActed ()
    with
    | ex -> onError ex


(* receive functions *)

let receiveWhileLoading (actions: IAggregateActorActions) behavior message =
    match message with
    | LoadState (state, version) ->
        match behavior.version with
        | 0 -> { behavior with state = state; version = version }
        | _ ->
            do actions.error MessageRejected "Cannot load aggregate snapshot after some events has been already loaded"
            behavior

    | LoadEvent (event, request) -> 
        let onError (ex: exn) =
            do actions.error ApplyFailed ex.Message
            { behavior with mode = Corrupted }
        applyEvent Common.idFun onError behavior event request
        
    | LoadError ex -> 
        do actions.error ErrorResult.LoadingFailed ex.Message
        { behavior with mode = Corrupted }

    | LoadDone -> 
        { behavior with mode = Receiving }

let receiveWhileReceiving (writer: IEventStoreWriter) (actions: IAggregateActorActions) behavior message =
    match message with 
    | ReceiveCommand ( command, expectedVersion, request ) ->
        if expectedVersion.IsSome && behavior.version <> expectedVersion.Value
        then
            do actions.error UnexpectedVersion (sprintf "Expected aggregate version was %d but %d found" expectedVersion.Value behavior.version)
            behavior
        else
            let onInvalid result =
                do actions.reject result
                behavior
            let onActed events =
                let writingS = 
                    WriteCommitInput.create request events None None
                    |> writer.writeCommit
                do actions.emit events request writingS
                { behavior with mode = Emitting }
            let onNotActed () =
                let text = sprintf "Command %A was not supposed to be received on current state" command
                do actions.reject (ValidationResult.create [ Warning text ])
                behavior
            let onValidateError (exn: exn) = 
                do actions.error ValidateFailed exn.Message
                behavior
            let onActError (exn: exn) = 
                do actions.error ActFailed exn.Message
                behavior
            let act () =
                actOnCommand onActed onNotActed onActError behavior behavior.state command request

            validateCommand act onInvalid onValidateError behavior command request

let receiveWhileEmitting (actions: IAggregateActorActions) behavior message =
    match message with
    | ApplyEvents (events, request) ->
        let onApplied newState =
            do actions.recover ()
            { newState with mode = Receiving }
        let onError (exn: exn) =
            do actions.error ApplyFailed exn.Message
            { behavior with mode = Corrupted }
        applyEvents onApplied onError behavior events request

(*     public functions     *)

let initialBehavior class' id = 
    {
        mode = Loading
        version = 0
        id = id
        class' = class'
        state = class'.init
        //pendingState = None
    }

let readCurrentEvents (reader: IEventStoreReader) aggregateRequest =
    let commitsS = 
        aggregateRequest 
        |> ReadCommitsInput.create 
        |> reader.readCommits

    let messagesS = 
        commitsS
            |> Obs.selectMany mapCommitToInputMessage

    messagesS

let receive writer actions behavior message = 
    behavior.mode |> function
    | Loading -> message |> function
        | LoadingMessage specific -> 
            receiveWhileLoading actions behavior specific
        | ReceivingMessage _ -> 
            do actions.postpone ()
            behavior
        | _ -> 
            do actions.error MessageRejected (sprintf "Cannot process message %A while loading events" message)
            behavior

    | Receiving -> message |> function
        | ReceivingMessage specific -> 
            receiveWhileReceiving writer actions behavior specific
        | _ -> 
            do actions.error MessageRejected (sprintf "Cannot process message %A while loading events" message)
            behavior

    | Emitting -> message |> function
        | EmittingMessage specific -> 
            receiveWhileEmitting actions behavior specific
        | ReceivingMessage _ -> 
            do actions.postpone ()
            behavior
        | _ -> 
            do actions.error MessageRejected (sprintf "Cannot process message %A while loading events" message)
            behavior

    | Corrupted ->
        do actions.error MessageRejected (sprintf "Cannot process message %A while corrupted" message)
        behavior
