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
open NUnit.Framework.Constraints

    


let aggregateModuleType = typedefof<TestingCommand>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = CommandRequest(new Map<string, obj>(seq []))
let aggregateId = "123456";
let initBehavior () = init aggregateClass aggregateId

let someBehavior2 initState initVersion messages =
    let rec someBehaviorAux state messages =
        match messages with 
        | [] -> state
        | msg :: tail ->
            let (ReceiveResult (_, nextState)) = receive state msg request
            someBehaviorAux nextState tail
    someBehaviorAux <| { initBehavior() with state = initState; version = initVersion } <| messages

let someBehavior = someBehavior2 testingInit 0

let testLoading loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> LoadEvent e)
    receive 
        <| someBehavior messages 
        <| message 
        <| request

let testLoadingState loadState loadVersion loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> LoadEvent e)
    receive 
        <| someBehavior2 loadState loadVersion messages 
        <| message 
        <| request

let testReceiving loadEvents message =  
    let messages = (loadEvents |> List.map (fun e -> LoadEvent e)) @ [ LoadDone ]
    receive 
        <| someBehavior messages 
        <| message 
        <| request

let testEmitting loadEvents command message =  
    let messages = (loadEvents |> List.map (fun e -> LoadEvent e)) @ [ LoadDone; ReceiveCommand (command, loadEvents.Length) ]
    receive 
        <| someBehavior messages 
        <| message 
        <| request

let testingApplyEvents events =  
    applyEvents3
        <| aggregateClass.init 
        <| aggregateClass.apply
        <| events
        <| request

let testingState mode version events =
    {
        initBehavior() with 
            mode = mode;
            version = version;
            state =  testingApplyEvents events
    }



[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))


[<Test>]
let ``Aggregate initial behavior state should be as expected`` () =
    initBehavior()
    |> should equal <| testingState Loading 0 []


(*  Loading  *)

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadEvent message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let (ReceiveResult (output, state)) = testLoading [] <| LoadEvent event
    output |> should equal MessageAccepted
    state |> should equal <| testingState Loading 1 [ event ]

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> List.fold (fun s e -> (testingApply e s)) testingInit
    let (ReceiveResult (output, state)) = testLoadingState snapshot events.Length [] <| LoadEvent event
    output |> should equal MessageAccepted
    state |> should equal <| testingState Loading 5 (events @ [ event ])

[<Test>]
let ``When Aggregate behavior, while Loading from version 5, if it receives a LoadState message, it should Reject it and continue Loading`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (_, midState)) = testLoading events <| LoadEvent (Incremented, 5)
    let (ReceiveResult (output, state)) = 
        receive 
        <| midState 
        <| (LoadState (midState.state, midState.version)) 
        <| request

    output |> should equal MessageRejected
    state |> should equal midState

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a failing event message, it should give a LoadingFailed response and continue Loading`` () =
    let event = FailingEvent
    let (ReceiveResult (output, state)) = testLoading [] <| LoadEvent event
    match output with
    | LoadingFailed e -> e |> should be instanceOfType<InvalidOperationException>
    state |> should equal <| testingState Loading 0 []

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadError message, it should give a LoadingFailed response and continue Loading`` () =
    let ex = InvalidOperationException "Failing event"
    let (ReceiveResult (output, state)) = testLoading [] <| LoadError ex 
    match output with
    | LoadingFailed e -> e |> should be instanceOfType<InvalidOperationException>
    state |> should equal <| testingState Loading 0 []

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadDone message, it should give a MessageAccepted response and continue Loading`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| LoadDone 
    output |> should equal MessageAccepted
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Loading, if it ReceiveCommand message, it should give a PostponeMessage response and continue in Loading state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| ReceiveCommand (Increment, 4)
    output |> should equal PostponeMessage
    state |> should equal <| testingState Loading 4 events

[<Test>]
let ``When Aggregate behavior, while Loading, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Loading state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| ApplyEvents [ Incremented ]
    output |> should equal MessageRejected
    state |> should equal <| testingState Loading 4 events


(*  Receiving  *)

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a valid command, it should give a EventsEmitted response and go to Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| ReceiveCommand (Increment, 4)
    output |> should equal <| EventsEmitted [ Incremented ]
    state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a command with unexpected version, it should give a UnexpectedVersion response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| ReceiveCommand (Increment, 3)
    output |> should equal UnexpectedVersion
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive an invalid command, it should give a IsInvalidCommand response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| ReceiveCommand (InvalidCommand, 4)
    match output with
    | OutputMessage.InvalidCommand e -> ()
    | _ -> Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a failing command, it should give a ActFailed response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| ReceiveCommand (FailingCommand, 4)
    match output with
    | OutputMessage.InvalidCommand v -> ()
    | _ -> Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| LoadEvent [ Incremented ]
    output |> should equal MessageRejected
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadError message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| LoadError (InvalidOperationException "message")
    output |> should equal MessageRejected
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadDone message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| LoadDone
    output |> should equal MessageRejected
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| ApplyEvents []
    output |> should equal MessageRejected
    state |> should equal <| testingState Receiving 4 events


(*  Receiving  *)

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message, it should give a MessageAccepted response, apply the events to behavior state and go to Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| ApplyEvents [ Incremented ]
    output |> should equal MessageAccepted
    state |> should equal <| testingState Receiving 5 (events @ [ Incremented ])

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message with failing event, it should give a ApplyFailed response and go to Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| ApplyEvents [ FailingEvent ]
    match output with
    | OutputMessage.ApplyFailed e -> ()
    | _ -> Assert.Fail(sprintf "Response should be ApplyFailed but was %A" output)
    state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| LoadEvent [ Decremented ]
    output |> should equal MessageRejected
    state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadError message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| LoadError (InvalidOperationException "message")
    output |> should equal MessageRejected
    state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadDone message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| LoadEvent [ Decremented ]
    output |> should equal MessageRejected
    state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ReceiveCommand message, it should give a PostponeMessage response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events Increment <| ReceiveCommand (Decrement, 5)
    output |> should equal PostponeMessage
    state |> should equal <| testingState Emitting 4 events
