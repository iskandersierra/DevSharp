module TestingAggregate

open System
open DevSharp.Annotations
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
| NopCommand

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
    | FailingCommand -> None
    | InvalidCommand -> Some [ Incremented ]

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
    | _ -> 
        seq []

