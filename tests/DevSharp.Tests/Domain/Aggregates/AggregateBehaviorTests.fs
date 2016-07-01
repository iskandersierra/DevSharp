module ``Aggregate behavior tests``

open System
open System.Reactive
open System.Reactive.Linq
open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit

open DevSharp
open DevSharp.Annotations
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open DevSharp.Server.Domain
open DevSharp.Validations.ValidationUtils

open TestingAggregate


let properties = 
    Map.empty
        .Add(AggregateIdConstant,      "my aggregate id" :> obj)
        // .Add(AggregateVersionConstant, 12345)
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
let initBehavior () = initialBehavior aggregateClass aggregateId

let newStoreReader commits = 
    new TestingEventStore(fun _ -> Observable.ToObservable<EventStoreCommit>(commits))
let newStoreWriter () = 
    new TestingEventStore(fun _ -> Observable.Empty<unit>())
let newActions() = TestingActions()


let someBehavior2 store actions initState initVersion messages =
    let rec someBehaviorAux state messages =
        match messages with 
        | [] -> state
        | msg :: tail ->
            let nextState = receive store actions state msg
            someBehaviorAux nextState tail
    someBehaviorAux { initBehavior() with state = initState; version = initVersion } messages

let someBehavior store actions = someBehavior2 store actions testingInit 0

let testLoading store actions loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)
    let behavior = someBehavior store actions messages
    receive store actions behavior message 

let testLoadingState store actions loadState loadVersion loadEvents message =  
    let messages = loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)
    let behavior = someBehavior2 store actions loadState loadVersion messages 
    receive store actions behavior message 

let testReceiving store actions loadEvents message =  
    let messages = (loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)) @ [ InputMessage.loadDone ]
    let behavior = someBehavior store actions messages 
    receive store actions behavior message 

let testEmitting store actions loadEvents command message =  
    let newMessages = [ 
        InputMessage.loadDone
        InputMessage.receiveCommand command (loadEvents |> List.length |> Some) request
    ]
    let loadMessages = loadEvents |> List.map (fun e -> InputMessage.loadEvent e request)
    let messages = loadMessages @ newMessages
    let behavior = someBehavior store actions messages
    receive store actions behavior message 

let testingApplyEvents events =  
    applyEvents3 aggregateClass.init aggregateClass.apply events request

let testingState mode version events =
    {
        initBehavior() with 
            mode = mode
            version = version
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
    // Given a store and actions
    // And event [ inc ] to load
    // When events are loaded
    // Then the state should be { Loading, ver = 1, {+1, -0} }
    // And no action should be received
    let store, actions = newStoreReader [], newActions ()
    let event = Incremented
    let state = testLoading store actions [] <| InputMessage.loadEvent event request
    let expectedState = testingState Loading 1 [ event ]
    do state |> should equal expectedState
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, it should Accept it and continue Loading`` () =
    // Given a store and actions
    // And a snapshot {+3,-1} to load
    // When the snapshot is loaded
    // Then the state should be { Loading, ver = 1, {+3, -1} }
    // And no action should be received
    let store, actions = newStoreReader [], newActions ()
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> List.fold (fun s e -> (testingApply e s)) testingInit
    let state = testLoadingState store actions snapshot events.Length []
    let expectedState = testingState Loading (events |> List.length) events
    do state |> should equal expectedState
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, and then a LoadEvent message, it should Accept them and continue Loading`` () =
    // Given a store and actions
    // And events [ inc, inc, dec, inc ] to load
    // And a snapshot {+3,-1} to load
    // And event [ inc ] to load after that
    // When the snapshot is loaded
    // And the event is loaded
    // Then the state should be { Loading, ver = 1, {+4, -1} }
    // And no action should be received
    let store, actions = newStoreReader [], newActions ()
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> List.fold (fun s e -> (testingApply e s)) testingInit
    let event = Incremented
    let state = testLoadingState store actions snapshot events.Length [] <| InputMessage.loadEvent event request
    do state |> should equal (testingState Loading 5 (events @ [ event ]))
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 5, if it receives a LoadState message, it should Reject it and continue Loading`` () =
    let store, actions = newStoreReader [], newActions ()
    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
    let midState = testLoading store actions events (InputMessage.loadEvent Incremented request)
    let state = 
        receive store actions
        <| midState 
        <| InputMessage.loadState midState.state midState.version 

    do state |> should equal midState
    do actions.calls |> should equal [ TestingActionCall.Error MessageRejected ]

//[<Test>]
//let ``When Aggregate behavior, while Loading, if it receives a failing event message, it should give a LoadingFailed response and continue Loading`` () =
//    let event = FailingEvent
//    let (ReceiveResult (output, state)) = testLoading [] <| InputMessage.loadEvent event request
//    match output with
//    | ExceptionMessage (e, LoadingFailed) -> do e |> should be instanceOfType<InvalidOperationException>
//    | _ -> do Assert.Fail "Should return an invalid operation exception"
//    do state |> should equal <| testingState Loading 0 []
//
//[<Test>]
//let ``When Aggregate behavior, while Loading, if it receives a LoadError message, it should give a LoadingFailed response and continue Loading`` () =
//    let ex = InvalidOperationException "Failing event"
//    let (ReceiveResult (output, state)) = testLoading [] <| InputMessage.loadError ex 
//    match output with
//    | ExceptionMessage (e, LoadingFailed) -> do e |> should be instanceOfType<InvalidOperationException>
//    | _ -> do Assert.Fail "Should return an invalid operation exception"
//    do state |> should equal <| testingState Loading 0 []
//
//[<Test>]
//let ``When Aggregate behavior, while Loading, if it receives a LoadDone message, it should give a MessageAccepted response and continue Loading`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.loadDone 
//    do output |> should equal OutputMessage.messageAccepted
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Loading, if it ReceiveCommand message, it should give a PostponeMessage response and continue in Loading state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.receiveCommand Increment 4 request
//    do output |> should equal PostponeMessage
//    do state |> should equal <| testingState Loading 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Loading, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Loading state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testLoading events <| InputMessage.applyEvents [ Incremented ] request
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Loading 4 events
//
//
//(*  Receiving  *)
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a valid command, it should give a EventsEmitted response and go to Emitting state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand Increment 4 request
//    do output |> should equal <| OutputMessage.eventsEmitted [ Incremented ] request
//    do state |> should equal <| testingState Emitting 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a command with unexpected version, it should give a UnexpectedVersion response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand Increment 3 request
//    do output |> should equal OutputMessage.unexpectedVersion
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive an invalid command, it should give a IsInvalidCommand response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand InvalidCommand 4 request
//    match output with
//    | OutputMessage.InvalidCommand e -> do ()
//    | _ -> do Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a failing command, it should give a ActFailed response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.receiveCommand FailingCommand 4 request
//    match output with
//    | OutputMessage.InvalidCommand v -> do ()
//    | _ -> do Assert.Fail(sprintf "Response should be InvalidCommand but was %A" output)
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadEvent [ Incremented ] request
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a LoadError message, it should give a MessageRejected response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadError (InvalidOperationException "message")
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a LoadDone message, it should give a MessageRejected response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.loadDone
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Receiving, if it receive a ApplyEvents message, it should give a MessageRejected response and continue in Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testReceiving events <| InputMessage.applyEvents [] request
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Receiving 4 events
//
//
//(*  Receiving  *)
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message, it should give a MessageAccepted response, apply the events to behavior state and go to Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.applyEvents [ Incremented ] request
//    do output |> should equal OutputMessage.messageAccepted
//    do state |> should equal <| testingState Receiving 5 (events @ [ Incremented ])
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a ApplyEvents message with failing event, it should give a ApplyFailed response and go to Receiving state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.applyEvents [ FailingEvent ] request
//    match output with
//    | ExceptionMessage (_, ApplyFailed) -> do ()
//    | _ -> do Assert.Fail(sprintf "Response should be ApplyFailed but was %A" output)
//    do state |> should equal <| testingState Receiving 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a LoadEvent message, it should give a MessageRejected response and continue in Emitting state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadEvent [ Decremented ] request
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Emitting 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a LoadError message, it should give a MessageRejected response and continue in Emitting state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadError (InvalidOperationException "message")
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Emitting 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a LoadDone message, it should give a MessageRejected response and continue in Emitting state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events <| Increment <| InputMessage.loadEvent [ Decremented ] request
//    do output |> should equal OutputMessage.messageRejected
//    do state |> should equal <| testingState Emitting 4 events
//
//[<Test>]
//let ``When Aggregate behavior, while Emitting, if it receive a ReceiveCommand message, it should give a PostponeMessage response and continue in Emitting state`` () =
//    let events = [ Incremented; Incremented; Decremented; Incremented; ] 
//    let (ReceiveResult (output, state)) = testEmitting events Increment <| InputMessage.receiveCommand Decrement 5 request
//    do output |> should equal OutputMessage.postponeMessage
//    do state |> should equal <| testingState Emitting 4 events
