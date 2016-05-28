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
| ProcessCommand    of CommandType * Request * AggregateVersion option
| OnNext            of EventType * Request
| OnError           of Exception
| OnDone

type OutputMessage =
| CommandProcessed  of EventType list
| InvalidCommand    of ValidationResult
| ValidateFailed    of Exception
| ActFailed         of Exception
| ApplyFailed       of Exception
| LoadingFailed     of Exception
| UnexpectedVersion
| PostponeMessage
| MessageRejected
| MessageAccepted

type Stage =
| LoadingEvents
| ProcessingCommands
| Failed

type BehaviorState =
    {
        stage: Stage;
        version: AggregateVersion;
        aggregateId: AggregateId;
        aggregateState: StateType;
        aggregateClass: IAggregateClass;
    }

type ReceiveResult =
| ReceiveResult of OutputMessage * BehaviorState

type ValidateCommandResult =
| IsValidCommand of ValidationResult
| IsInvalidCommand of ValidationResult
| ValidationHasFailed of Exception

type ApplyEventResult =
| EventWasApplied of BehaviorState
| ApplyEventHasFailed of Exception

type ActOnCommandResult =
| CommandWasActedOn of EventType list
| ActOnCommandHasFailed of Exception

let init aggregateClass aggregateId = 
    {
        stage = LoadingEvents;
        version = 0;
        aggregateId = aggregateId;
        aggregateClass = aggregateClass;
        aggregateState = aggregateClass.init;
    }

let receive message state = 

    let validateCommand cmd req =
        try
            let result = state.aggregateClass.validate cmd req
            if result.isValid 
            then IsValidCommand result
            else IsInvalidCommand result
        with 
        | _ as ex -> ValidationHasFailed ex

    let applyEvent state event request =
        try
            let newState = state.aggregateClass.apply event state.aggregateState request
            EventWasApplied { 
                state with  
                    version = state.version + 1;
                    aggregateState = newState;
            }
        with
        | _ as ex -> ApplyEventHasFailed ex

    let rec applyEvents state events request =
        match events with
        | [] -> EventWasApplied state
        | e :: xs ->
            match applyEvent state e request with
            | ApplyEventHasFailed ex -> ApplyEventHasFailed ex
            | EventWasApplied newState -> applyEvents newState xs request

    let actOnCommand command request =
        try
            let events = state.aggregateClass.act command state.aggregateState request
            CommandWasActedOn ( Seq.toList events )
        with
        | _ as ex ->
            ActOnCommandHasFailed ex

    let whenLoadingEvents() =
        match message with
        | OnNext (event, request) -> 
            match applyEvent state event request with
            | EventWasApplied newState -> ReceiveResult (MessageAccepted, newState)
            | ApplyEventHasFailed ex -> ReceiveResult (LoadingFailed ex, state)
        
        | OnError ex -> ReceiveResult (LoadingFailed ex, { state with stage = ProcessingCommands })
        | OnDone -> ReceiveResult (MessageAccepted, { state with stage = ProcessingCommands })
        | ProcessCommand _ -> ReceiveResult (PostponeMessage, state)

    let versionMatches version =
        match version with
        | None -> true
        | Some ver -> ver = state.version

    let whenProcessingCommands() =
        match message with
        | ProcessCommand ( command, request, version ) ->
            if not <| versionMatches version
            then ReceiveResult (UnexpectedVersion, state)
            else
                match validateCommand command request with
                | ValidationHasFailed ex -> ReceiveResult (ValidateFailed ex, state)
                | IsInvalidCommand result -> ReceiveResult (InvalidCommand result, state)
                | IsValidCommand _ -> 
                    match actOnCommand command request with
                    | ActOnCommandHasFailed ex -> ReceiveResult (ActFailed ex, state)
                    | CommandWasActedOn events ->
                        
        | _ -> ReceiveResult (MessageRejected, state)

    match state.stage with
    | LoadingEvents ->
        whenLoadingEvents()

    | ProcessingCommands ->
        whenProcessingCommands()

    | Failed ->
        ReceiveResult (MessageRejected, state)
