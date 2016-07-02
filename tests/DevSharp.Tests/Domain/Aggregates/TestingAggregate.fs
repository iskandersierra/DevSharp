module TestingAggregate

open System
open DevSharp
open DevSharp.Annotations
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.Validations.ValidationUtils


[<AggregateEvent>]
type TestingEvent = 
    | FailingEvent 
    | Incremented 
    | Decremented

[<AggregateCommand>]
type TestingCommand = 
    | FailingCommand 
    | Increment 
    | Decrement 
    | InvalidCommand 
    | ValidateFailCommand
    | NopCommand
    | DoNotActCommand

type TestingState = 
    { 
        incCount: int
        decCount: int
    }

[<AggregateInit>]
let testingInit = { incCount = 0; decCount = 0; }

[<AggregateAct>]
let testingAct command = 
    match command with
    | Increment -> Some [ Incremented ]
    | Decrement -> Some [ Decremented ]
    | NopCommand -> Some [ ]
    | FailingCommand -> raise (InvalidOperationException "Failing command")
    | InvalidCommand -> Some [ Incremented ]
    | DoNotActCommand -> None
    | ValidateFailCommand -> Some [ ]

[<AggregateApply>]
let testingApply event state = 
    match event with
    | Incremented -> { state with incCount = state.incCount + 1 }
    | Decremented -> { state with decCount = state.decCount + 1 }
    | FailingEvent -> raise (InvalidOperationException "Failing event")

[<AggregateValidate>]
let testingValidate command =
    match command with
    | InvalidCommand -> 
        seq { yield memberFailure "id" "Id must be positive" }
    | ValidateFailCommand -> 
        raise (InvalidOperationException "Failing validate")
    | _ -> 
        seq []
