module DevSharp.Domain.Aggregates.AggregateBehavior

open System
open DevSharp.Messaging
open DevSharp.Validations


type AggregateId      = string // guid
type AggregateVersion = int
type CommandType      = obj
type EventType        = obj
type StateType        = obj

type InputMessage =
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

let receiveResult output behavior = 
    ReceiveResult (output, behavior)


(*     Internal functions      *)



type ValidateCommandResult =
| IsValidCommand of ValidationResult
| IsInvalidCommand of ValidationResult
| ValidationHasFailed of Exception

let validateCommand behavior command request =
    try
        let result = behavior.class'.validate command request
        if result.isValid 
        then IsValidCommand result
        else IsInvalidCommand result
    with 
    | _ as ex -> ValidationHasFailed ex

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
        | ApplyEventHasFailed ex -> ApplyEventHasFailed ex
        | EventWasApplied newState -> applyEvents newState tail request

type ActOnCommandResult =
| CommandWasActedOn of EventType list
| ActOnCommandHasFailed of Exception

let actOnCommand bahavior state command request =
    try
        let events = bahavior.class'.act command state request
        CommandWasActedOn ( Seq.toList events )
    with
    | _ as ex ->
        ActOnCommandHasFailed ex



let receiveWhileLoading behavior message request =
    match message with
    | LoadEvent event -> 
        match applyEvent behavior event request with
        | EventWasApplied newState -> receiveResult MessageAccepted newState
        | ApplyEventHasFailed ex -> receiveResult <| LoadingFailed ex <| behavior
        
    | LoadError ex -> receiveResult <| LoadingFailed ex <| behavior
    | LoadDone -> receiveResult MessageAccepted { behavior with mode = Receiving }
    | ReceiveCommand _ -> receiveResult PostponeMessage behavior
    | ApplyEvents _ -> receiveResult MessageRejected behavior

let receiveWhileReceiving behavior message request =
    match message with
    | ReceiveCommand ( command, expectedVersion ) ->
        if behavior.version <> expectedVersion
        then receiveResult UnexpectedVersion behavior
        else
            match validateCommand behavior command request with
            | ValidationHasFailed ex -> receiveResult (ValidateFailed ex) behavior
            | IsInvalidCommand result -> receiveResult (InvalidCommand result) behavior
            | IsValidCommand _ -> 
                match actOnCommand behavior behavior.state command request with
                | ActOnCommandHasFailed ex -> receiveResult (ActFailed ex) behavior
                | CommandWasActedOn events -> receiveResult (EventsEmitted events) { behavior with mode = Emitting }
                        
    | _ -> receiveResult MessageRejected behavior

let receiveWhileEmitting behavior message request =
    match message with
    | ApplyEvents events -> 
        match applyEvents behavior events request with
        | EventWasApplied newState -> receiveResult MessageAccepted { newState with mode = Receiving }
        | ApplyEventHasFailed ex -> receiveResult (ApplyFailed ex) { behavior with mode = Receiving }
    | ReceiveCommand _ -> receiveResult PostponeMessage behavior
    | _ -> receiveResult MessageRejected behavior


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
