module ``Aggregate behavior tests``

open NUnit.Framework
open System
open FsUnit
open DevSharp.Annotations
open DevSharp
open DevSharp.Validations.ValidationUtils
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open DevSharp.Server.Domain
open TestingAggregate
open NUnit.Framework.Constraints

    


let properties = 
    Map.empty
        .Add(AggregateIdConstant,      "my aggregate id" :> obj)
        .Add(AggregateVersionConstant, 12345 :> obj)
        .Add(ApplicationIdConstant,    "my application id" :> obj)
        .Add(AggregateTypeConstant,    "my aggregate type" :> obj)
        .Add(ProjectIdConstant,        "my project id" :> obj)
        .Add(CommandIdConstant,        "my command id" :> obj)
        .Add(CommandTypeConstant,      "my command type" :> obj)
        .Add(SessionIdConstant,        "my session id" :> obj)
        .Add(TenantIdConstant,         "my tenant id" :> obj)
        .Add(UserIdConstant,           "my user id" :> obj)
        .Add(ClientDateConstant,       RequestDate.Now :> obj)
        .Add(ApiDateConstant,          RequestDate.Now :> obj)
        .Add(ProcessDateConstant,      RequestDate.Now :> obj)
let aggregateModuleType = typedefof<TestingCommand>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = (toCommandRequest properties).Value
let aggregateId = "123456";
let initBehavior () = init aggregateClass aggregateId

let someBehavior2 initState initVersion messages =
    let rec someBehaviorAux state messages =
        match messages with 
        | [] -> state
        | msg :: tail ->
            let (ReceiveResult (_, nextState)) = receive state msg
            someBehaviorAux nextState tail
    someBehaviorAux <| { initBehavior() with state = initState; version = initVersion } <| messages

let someBehavior = someBehavior2 testingInit 0

let testLoading loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)
    receive 
        <| someBehavior messages 
        <| message 

let testLoadingState loadState loadVersion loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)
    receive 
        <| someBehavior2 loadState loadVersion messages 
        <| message 

let testReceiving loadEvents message =  
    let messages = (loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)) @ [ InputMessage.loadDone ]
    receive 
        <| someBehavior messages 
        <| message 

let testEmitting loadEvents command message =  
    let newMessages = [ 
        InputMessage.loadDone
        InputMessage.receiveCommand command (loadEvents |> List.length) request
    ]
    let loadMessages =
        loadEvents 
        |> List.map (fun e -> InputMessage.loadEvent e request)
    let messages = loadMessages @ newMessages
    receive 
        <| someBehavior messages 
        <| message 

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
    |> should equal (testingState Loading 0 [])


(*  Loading  *)

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadEvent message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let (ReceiveResult (output, state)) = testLoading [] <| InputMessage.loadEvent event request
    do output |> should equal OutputMessage.messageAccepted
    do state |> should equal (testingState Loading 1 [ event ])

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, it should Accept it and continue Loading`` () =
    let event = Incremented
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> List.fold (fun s e -> (testingApply e s)) testingInit
    let (ReceiveResult (output, state)) = testLoadingState snapshot events.Length [] <| InputMessage.loadEvent event request
    do output |> should equal OutputMessage.messageAccepted
    do state |> should equal <| testingState Loading 5 (events @ [ event ])

[<Test>]
let ``When Aggregate behavior, while Loading from version 5, if it receives a LoadState message, it should Reject it and continue Loading`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (_, midState)) = testLoading events (InputMessage.loadEvent Incremented request)
    let (ReceiveResult (output, state)) = 
        receive 
        <| midState 
        <| InputMessage.loadState midState.state midState.version 

    do output |> should equal OutputMessage.messageRejected
    do state |> should equal midState

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a failing event message, it should give a LoadingFailed response and continue Loading`` () =
    let event = FailingEvent
    let (ReceiveResult (output, state)) = testLoading [] <| InputMessage.loadEvent event request
    match output with
    | ExceptionMessage (e, LoadingFailed) -> do e |> should be instanceOfType<InvalidOperationException>
    | _ -> do Assert.Fail "Should return an invalid operation exception"
    do state |> should equal <| testingState Loading 0 []

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadError message, it should give a LoadingFailed response and continue Loading`` () =
    let ex = InvalidOperationException "Failing event"
    let (ReceiveResult (output, state)) = testLoading [] <| InputMessage.loadError ex 
    match output with
    | ExceptionMessage (e, LoadingFailed) -> do e |> should be instanceOfType<InvalidOperationException>
    | _ -> do Assert.Fail "Should return an invalid operation exception"
    do state |> should equal <| testingState Loading 0 []

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadDone message, it should give a MessageAccepted response and continue Loading`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.loadDone 
    do output |> should equal OutputMessage.messageAccepted
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Loading, if it ReceiveCommand message, it should give a PostponeMessage response and continue in Loading state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.receiveCommand Increment 4 request
    do output |> should equal PostponeMessage
    do state |> should equal <| testingState Loading 4 events

[<Test>]
let ``When Aggregate behavior, while Loading, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Loading state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.applyEvents [ Incremented ] request
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Loading 4 events


(*  Receiving  *)

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a valid command, it should give a EventsEmitted response and go to Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand Increment 4 request
    do output |> should equal <| EventsEmitted [ Incremented ]
    do state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a command with unexpected version, it should give a UnexpectedVersion response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand Increment 3 request
    do output |> should equal OutputMessage.unexpectedVersion
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive an invalid command, it should give a IsInvalidCommand response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand InvalidCommand 4 request
    match output with
    | OutputMessage.InvalidCommand e -> do ()
    | _ -> do Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a failing command, it should give a ActFailed response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand FailingCommand 4 request
    match output with
    | OutputMessage.InvalidCommand v -> do ()
    | _ -> do Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadEvent [ Incremented ] request
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadError message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadError (InvalidOperationException "message")
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a LoadDone message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadDone
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.applyEvents [] request
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Receiving 4 events


(*  Receiving  *)

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message, it should give a MessageAccepted response, apply the events to behavior state and go to Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.applyEvents [ Incremented ] request
    do output |> should equal OutputMessage.messageAccepted
    do state |> should equal <| testingState Receiving 5 (events @ [ Incremented ])

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message with failing event, it should give a ApplyFailed response and go to Receiving state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.applyEvents [ FailingEvent ] request
    match output with
    | ExceptionMessage (_, ApplyFailed) -> do ()
    | _ -> do Assert.Fail(sprintf "Response should be ApplyFailed but was %A" output)
    do state |> should equal <| testingState Receiving 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadEvent [ Decremented ] request
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadError message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadError (InvalidOperationException "message")
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a LoadDone message, it should give a MessageRejected response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadEvent [ Decremented ] request
    do output |> should equal OutputMessage.messageRejected
    do state |> should equal <| testingState Emitting 4 events

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receive a ReceiveCommand message, it should give a PostponeMessage response and continue in Emitting state`` () =
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let (ReceiveResult (output, state)) = testEmitting events Increment <| InputMessage.receiveCommand Decrement 5 request
    do output |> should equal OutputMessage.postponeMessage
    do state |> should equal <| testingState Emitting 4 events
