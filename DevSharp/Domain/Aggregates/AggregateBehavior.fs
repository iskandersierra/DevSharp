module DevSharp.Domain.Aggregates.AggregateBehavior

open System
open DevSharp.Messaging
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils


type AggregateId      = string // guid
type AggregateVersion = int
type CommandType      = obj
type EventType        = obj
type StateType        = obj

type InputMessage =
| LoadState         of StateType * AggregateVersion
| LoadEvent         of EventType
| LoadError         of Exception
| LoadDone
| ReceiveCommand    of CommandType * AggregateVersion
| ApplyEvents       of EventType list

type OutputMessage =
| CommandDone
| EventsEmitted     of EventType list
| InvalidCommand    of ValidationResult
| ValidateFailed    of Exception
| ActFailed         of Exception
| ApplyFailed       of Exception
| LoadingFailed     of Exception
| UnexpectedVersion
| PostponeMessage
| MessageRejected
| MessageAccepted

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

let receiveResult output behavior = ReceiveResult (output, behavior)
let eventsEmitted events behavior = receiveResult (EventsEmitted events) behavior
let invalidCommand result behavior = receiveResult (InvalidCommand result) behavior
let validateFailed ex behavior = receiveResult (ValidateFailed ex) behavior
let actFailed ex behavior = receiveResult (ActFailed ex) behavior
let applyFailed ex behavior = receiveResult (ApplyFailed ex) behavior
let loadingFailed ex behavior = receiveResult (LoadingFailed ex) behavior


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



let receiveWhileLoading behavior message request =
    match message with
    | LoadState (state, version) ->
        if behavior.version = 0 
        then receiveResult MessageAccepted { behavior with state = state; version = version }
        else receiveResult MessageRejected behavior

    | LoadEvent event -> 
        match applyEvent behavior event request with
        | EventWasApplied newState -> 
            receiveResult MessageAccepted newState
        | ApplyEventHasFailed ex -> 
            loadingFailed ex behavior
        
    | LoadError ex -> 
        loadingFailed ex behavior
    | LoadDone -> 
        receiveResult MessageAccepted { behavior with mode = Receiving }
    | ReceiveCommand _ -> 
        receiveResult PostponeMessage behavior
    | ApplyEvents _ -> 
        receiveResult MessageRejected behavior

let receiveWhileReceiving behavior message request =
    match message with
    | ReceiveCommand ( command, expectedVersion ) ->
        if behavior.version <> expectedVersion
        then receiveResult UnexpectedVersion behavior
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
                        
    | _ -> receiveResult MessageRejected behavior

let receiveWhileEmitting behavior message request =
    match message with
    | ApplyEvents events -> 
        match applyEvents behavior events request with
        | EventWasApplied newState -> 
            receiveResult MessageAccepted { newState with mode = Receiving }
        | ApplyEventHasFailed ex -> 
            applyFailed ex { behavior with mode = Receiving }
    | ReceiveCommand _ -> 
        receiveResult PostponeMessage behavior
    | _ -> 
        receiveResult MessageRejected behavior


(*     init & receive      *)



let init class' id = 
    {
        mode = Loading;
        version = 0;
        id = id;
        class' = class';
        state = class'.init;
    }

let receive behavior message request = 
    match behavior.mode with
    | Loading ->
        receiveWhileLoading behavior message request

    | Receiving ->
        receiveWhileReceiving behavior message request

    | Emitting ->
        receiveWhileEmitting behavior message request
