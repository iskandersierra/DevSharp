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
| EventsPersisted   of EventType list

type OutputMessage =
| CommandDone
| PersistEvents     of EventType list
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
| Waiting
| Persisting
| Failed

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

let applyEvent behavior state event request =
    try
        let newState = behavior.class'.apply event state request
        EventWasApplied { 
            behavior with  
                version = behavior.version + 1;
                state = newState;
        }
    with
    | _ as ex -> ApplyEventHasFailed ex

let rec applyEvents behavior state events request =
    match events with
    | [] -> EventWasApplied state
    | event :: tail ->
        match applyEvent behavior state event request with
        | ApplyEventHasFailed ex -> ApplyEventHasFailed ex
        | EventWasApplied newState -> applyEvents behavior newState tail request

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



(*     init & receive      *)



let init class' id = 
    {
        mode = Loading;
        version = 0;
        id = id;
        class' = class';
        state = class'.init;
    }



let receiveWhileLoading behavior message request =
    match message with
    | LoadEvent event -> 
        match applyEvent behavior behavior.state event request with
        | EventWasApplied newState -> receiveResult MessageAccepted newState
        | ApplyEventHasFailed ex -> receiveResult <| LoadingFailed ex <| { behavior with mode = Failed }
        
    | LoadError ex -> receiveResult <| LoadingFailed ex <| { behavior with mode = Failed }
    | LoadDone -> receiveResult MessageAccepted { behavior with mode = Waiting }
    | ReceiveCommand _ -> receiveResult PostponeMessage behavior
    | EventsPersisted _ -> receiveResult MessageRejected behavior

//let receiveWhileWaiting behavior message =
//    match message with
//    | OnCommand ( command, request, expectedVersion ) ->
//        if behavior.version <> expectedVersion
//        then ReceiveResult (UnexpectedVersion, behavior)
//        else
//            match validateCommand behavior command request with
//            | ValidationHasFailed ex -> ReceiveResult (ValidateFailed ex, behavior)
//            | IsInvalidCommand result -> ReceiveResult (InvalidCommand result, behavior)
//            | IsValidCommand _ -> 
//                match actOnCommand behavior command request with
//                | ActOnCommandHasFailed ex -> ReceiveResult (ActFailed ex, state)
//                | CommandWasActedOn events ->
//                        
//    | _ -> ReceiveResult (MessageRejected, state)
//
//let receive behavior message = 
//
//    let whenProcessingCommands() =
//        match message with
//        | ProcessCommand ( command, request, version ) ->
//            if not <| versionMatches version
//            then ReceiveResult (UnexpectedVersion, state)
//            else
//                match validateCommand command request with
//                | ValidationHasFailed ex -> ReceiveResult (ValidateFailed ex, state)
//                | IsInvalidCommand result -> ReceiveResult (InvalidCommand result, state)
//                | IsValidCommand _ -> 
//                    match actOnCommand command request with
//                    | ActOnCommandHasFailed ex -> ReceiveResult (ActFailed ex, state)
//                    | CommandWasActedOn events ->
//                        
//        | _ -> ReceiveResult (MessageRejected, state)
//
//    match state.mode with
//    | LoadingEvents ->
//        whenLoadingEvents()
//
//    | ProcessingCommands ->
//        whenProcessingCommands()
//
//    | Failed ->
//        ReceiveResult (MessageRejected, state)

let receive behavior message request = 
    match behavior.mode with
    | Loading ->
        receiveWhileLoading behavior message request
    | _ ->
        failwith "Not implemented yet"