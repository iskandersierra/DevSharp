module ``Aggregate behavior tests``

open NUnit.Framework
open System
open FsUnit
open DevSharp.Annotations
open DevSharp.Messaging
open DevSharp.Validations.ValidationUtils
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open DevSharp.Server.Domain
open TestingAggregate

    


let aggregateModuleType = typedefof<TestingCommand>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = Request(new Map<string, obj>(seq []))
let aggregateId = "123456";
let initBehavior() = init aggregateClass aggregateId
let someBehavior messages =
    let rec someBehaviorAux state messages =
        match messages with 
        | [] -> state
        | msg :: tail ->
            let (ReceiveResult (_, nextState)) = receive state msg request
            someBehaviorAux nextState tail
    someBehaviorAux <| initBehavior() <| messages


[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)


[<Test>]
let ``Aggregate initial behavior state should be as expected`` () =
    initBehavior()
    |> should equal 
        {
            class' = aggregateClass;
            id = aggregateId;
            mode = Loading;
            version = 0;
            state = aggregateClass.init;
        }


[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a regular event message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let response = receive 
                <| initBehavior()
                <| LoadEvent event 
                <| request
    match response with
    | ReceiveResult (output, state) ->
        output |> should equal MessageAccepted
        state |> should equal 
            {
                initBehavior() with 
                    mode = Loading;
                    version = 1;
                    state = aggregateClass.apply event aggregateClass.init request;
            }

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a failing event message, it should give a LoadingFailed response and pass to Failed state`` () =
    let event = FailingEvent
    let response = receive 
                <| initBehavior()
                <| LoadEvent event 
                <| request
    match response with
    | ReceiveResult (output, state) ->
        match output with
        | LoadingFailed e -> e |> should be instanceOfType<InvalidOperationException>
        state |> should equal 
            {
                initBehavior() with 
                    mode = Failed;
                    version = 0;
                    state = aggregateClass.init;
            }

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadError event message, it should give a LoadingFailed response and pass to Failed state`` () =
    let ex = InvalidOperationException "Failing event"
    let response = receive 
                <| initBehavior()
                <| LoadError ex 
                <| request
    match response with
    | ReceiveResult (output, state) ->
        match output with
        | LoadingFailed e -> e |> should be instanceOfType<InvalidOperationException>
        state |> should equal 
            {
                initBehavior() with 
                    mode = Failed;
                    version = 0;
                    state = aggregateClass.init;
            }

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadDone event message, it should give a MessageAccepted response and pass to Waiting state`` () =
    let response = receive 
                <| someBehavior [ LoadEvent Incremented; LoadEvent Incremented; LoadEvent Decremented; LoadEvent Incremented; ] 
                <| LoadDone 
                <| request

    match response with
    | ReceiveResult (output, state) ->
        output |> should equal MessageAccepted
        state |> should equal 
            {
                initBehavior() with 
                    mode = Waiting;
                    version = 4;
                    state =  applyEvents3
                        <| aggregateClass.init 
                        <| aggregateClass.apply
                        <| [ Incremented; Incremented; Decremented; Incremented; ]
                        <| request
            }



