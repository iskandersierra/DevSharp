module ``Aggregate behavior tests``

open NUnit.Framework
open FsUnit
open DevSharp.Annotations
open DevSharp.Messaging
open DevSharp.Validations.ValidationUtils
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open DevSharp.Server.Domain



[<AggregateEvent>]
type TestingEvent = FailingEvent | Incremented | Decremented | UnknownEvent
[<AggregateCommand>]
type TestingCommand = FailingCommand | Increment | Decrement | UnknownCommand
type TestingState = { incCount: int; decCount: int; }
[<AggregateInit>]
let testingInit = { incCount = 0; decCount = 0; }
[<AggregateAct>]
let testingAct command = 
    match command with
    | Increment -> [ Incremented ]
    | Decrement -> [ Decremented ]
    | FailingCommand -> failwith "Failing command"
[<AggregateApply>]
let apply event state = 
    match event with
    | Incremented -> { state with incCount = state.incCount + 1; }
    | Decremented -> { state with decCount = state.decCount + 1; }
    | FailingEvent -> failwith "Failing event"
let validate command =
    seq {
        yield memberFailure "id" "Id must be positive"
    }    
    


let aggregateModuleType = typedefof<TestingCommand>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = Request(new Map<string, obj>(seq []))
let aggregateId = "123456";
let initBehavior() = init aggregateClass aggregateId


[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)


[<Test>]
let ``Aggregate behavior state should be as expected`` () =
    initBehavior()
    |> should equal 
        {
            mode = Loading;
            version = 0;
            id = aggregateId;
            class' = aggregateClass;
            state = aggregateClass.init;
        }


[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a OnNext event message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let response = OnNext (event, request) |> receive (initBehavior())
    match response with
    | ReceiveResult (output, state) ->
        output |> should equal MessageAccepted
        state |> should equal 
            {
                mode = Loading;
                version = 1;
                id = aggregateId;
                class' = aggregateClass;
                state = aggregateClass.apply event aggregateClass.init request;
            }


