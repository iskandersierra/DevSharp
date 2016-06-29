module DevSharp.Domain.Aggregates.AggregateBehavior

open System
open DevSharp
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
| ReceiveCommand     of CommandType * AggregateVersion * CommandRequest
and EmittingMessage  =
| ApplyEvents        of EventType list * CommandRequest

type OutputMessage =
| SuccessMessage    of SuccessMessage
| EventsEmitted     of EventType list
| InvalidCommand    of ValidationResult
| ExceptionMessage  of Exception * ExceptionMessage
| RejectionMessage  of RejectionMessage
| PostponeMessage
with
    static member commandDone               = SuccessMessage   CommandDone
    static member messageAccepted           = SuccessMessage   MessageAccepted
    static member unexpectedVersion         = RejectionMessage UnexpectedVersion
    static member messageRejected           = RejectionMessage MessageRejected
    static member eventsEmitted events      = EventsEmitted    events
    static member invalidCommand validation = InvalidCommand   validation
    static member validateFailed exn        = ExceptionMessage (exn, ValidateFailed)
    static member actFailed exn             = ExceptionMessage (exn, ActFailed)
    static member applyFailed exn           = ExceptionMessage (exn, ApplyFailed)
    static member loadingFailed exn         = ExceptionMessage (exn, LoadingFailed)
    static member postponeMessage           = PostponeMessage
and SuccessMessage =
| CommandDone
| MessageAccepted
and RejectionMessage =
| UnexpectedVersion
| MessageRejected
and ExceptionMessage =
| ValidateFailed
| ActFailed
| ApplyFailed
| LoadingFailed


type Mode =
| Loading
| Receiving
| Emitting


type BehaviorState =
    {
        mode: Mode;
        version: AggregateVersion;
        id: AggregateId;
        state: StateType;
        class': IAggregateClass;
    }

type ReceiveResult =
| ReceiveResult of OutputMessage * BehaviorState

let private receiveResult output behavior = ReceiveResult (output, behavior)
let private eventsEmitted events behavior = receiveResult (OutputMessage.eventsEmitted events) behavior
let private invalidCommand result behavior = receiveResult (OutputMessage.invalidCommand result) behavior
let private validateFailed ex behavior = receiveResult (OutputMessage.validateFailed ex) behavior
let private actFailed ex behavior = receiveResult (OutputMessage.actFailed ex) behavior
let private applyFailed ex behavior = receiveResult (OutputMessage.applyFailed ex) behavior
let private loadingFailed ex behavior = receiveResult (OutputMessage.loadingFailed ex) behavior


(*     Internal functions      *)


type ValidateCommandResult =
| IsValidCommand of ValidationResult
| IsInvalidCommand of ValidationResult
| ValidationHasFailed of Exception

let validateCommand behavior command request =
    try
        let result = behavior.class'.validate command request
        match result.isValid with
        | true -> 
            IsValidCommand result
        | false -> 
            IsInvalidCommand result
    with 
    | _ as ex -> 
        ValidationHasFailed ex

type ApplyEventResult =
| EventWasApplied of BehaviorState
| ApplyEventHasFailed of Exception

let applyEvent behavior event request =
    try
        let newState = behavior.class'.apply event behavior.state request
        EventWasApplied { 
            behavior with  
                version = behavior.version + 1;
                state = newState;
        }
    with
    | _ as ex -> ApplyEventHasFailed ex

let rec applyEvents behavior events request =
    match events with
    | [] -> EventWasApplied behavior
    | event :: tail ->
        match applyEvent behavior event request with
        | ApplyEventHasFailed ex -> 
            ApplyEventHasFailed ex
        | EventWasApplied newState -> 
            applyEvents newState tail request

type ActOnCommandResult =
| CommandWasActedOn of EventType list
| CommandWasNotActedOn
| ActOnCommandHasFailed of Exception

let actOnCommand bahavior state command request =
    try
        let maybeEvents = bahavior.class'.act command state request
        match maybeEvents with
        | Some events -> 
            CommandWasActedOn ( Seq.toList events )
        | None -> 
            CommandWasNotActedOn
    with
    | _ as ex ->
        ActOnCommandHasFailed ex



let receiveWhileLoading behavior message =
    match message with
    | LoadState (state, version) ->
        if behavior.version = 0 
        then receiveResult OutputMessage.messageAccepted { behavior with state = state; version = version }
        else receiveResult OutputMessage.messageRejected behavior

    | LoadEvent (event, request) -> 
        match applyEvent behavior event request with
        | EventWasApplied newState -> 
            receiveResult OutputMessage.messageAccepted newState
        | ApplyEventHasFailed ex -> 
            loadingFailed ex behavior
        
    | LoadError ex -> 
        loadingFailed ex behavior
    | LoadDone -> 
        receiveResult OutputMessage.messageAccepted { behavior with mode = Receiving }

let receiveWhileReceiving behavior message =
    match message with
    | ReceiveCommand ( command, expectedVersion, request ) ->
        if behavior.version <> expectedVersion
        then receiveResult OutputMessage.unexpectedVersion behavior
        else
            match validateCommand behavior command request with
            | ValidationHasFailed ex -> 
                validateFailed ex behavior
            | IsInvalidCommand result -> 
                invalidCommand result behavior
            | IsValidCommand _ -> 
                match actOnCommand behavior behavior.state command request with
                | ActOnCommandHasFailed ex -> 
                    actFailed ex behavior
                | CommandWasActedOn events -> 
                    eventsEmitted events { behavior with mode = Emitting }
                | CommandWasNotActedOn -> 
                    invalidCommand (commandFailureResult "Command was not supposed to be received on current state") behavior

let receiveWhileEmitting behavior message =
    match message with
    | ApplyEvents (events, request) -> 
        match applyEvents behavior events request with
        | EventWasApplied newState -> 
            receiveResult OutputMessage.messageAccepted { newState with mode = Receiving }
        | ApplyEventHasFailed ex -> 
            applyFailed ex { behavior with mode = Receiving }


(*     init & receive      *)

let init class' id = 
    {
        mode = Loading;
        version = 0;
        id = id;
        class' = class';
        state = class'.init;
    }

let receive behavior message = 
    behavior.mode |> function
    | Loading -> message |> function
        | LoadingMessage specific -> receiveWhileLoading behavior specific
        | ReceivingMessage _ -> receiveResult PostponeMessage behavior
        | _ -> receiveResult OutputMessage.messageRejected behavior

    | Receiving -> message |> function
        | ReceivingMessage specific -> receiveWhileReceiving behavior specific
        | _ -> receiveResult OutputMessage.messageRejected behavior

    | Emitting -> message |> function
        | EmittingMessage specific -> receiveWhileEmitting behavior specific
        | ReceivingMessage _ -> receiveResult PostponeMessage behavior
        | _ -> receiveResult OutputMessage.messageRejected behavior
