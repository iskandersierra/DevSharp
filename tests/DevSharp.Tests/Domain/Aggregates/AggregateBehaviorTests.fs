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
    let fromState = { initBehavior() with state = initState; version = initVersion }
    messages |> List.fold (receive store actions) fromState

let someBehavior store actions = 
    someBehavior2 store actions testingInit 0

let foldBehavior = List.fold (fun s e -> (testingApply e s)) testingInit

let testingApplyEvents events =  
    applyEvents3 aggregateClass.init aggregateClass.apply events request

let testingState mode version events =
    {
        initBehavior() with 
            mode = mode
            version = version
            state = testingApplyEvents events
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
let ``When Aggregate behavior, while Loading, if it receives a LoadEvent message, it should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And event [ inc ] to load
    let event = Incremented
    // And an initial state
    // When the LoadEvent message is sent
    let state = receive store actions (initBehavior()) (InputMessage.loadEvent event request)
    // Then the state should be { Loading, ver = 1, {+1, -0} }
    let expectedState = testingState Loading 1 [ event ]
    do state |> should equal expectedState
    // And no action should be received
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, it should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // Then the state should be { Loading, ver = 4, {+3, -1} }
    let expectedState = testingState Loading 4 events
    do state |> should equal expectedState
    // And no action should be received
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 0, if it receives a LoadState message, and then a LoadEvent message, it should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // And event [ inc ] to load after that
    let event = Incremented
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the event is loaded
    let state = receive store actions state1 (InputMessage.loadEvent event request)
    // Then the state should be { Loading, ver = 1, {+4, -1} }
    do state |> should equal (testingState Loading 5 (events @ [ event ]))
    // And no action should be received
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading from version 4, if it receives a LoadState message, it should signal an error of MessageRejected and should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the event is loaded
    let state = receive store actions state1 (InputMessage.loadState snapshot 4)
    // Then the state should be the same
    do state |> should equal state1
    // And a rejected action should be registered
    do actions.calls |> should equal [ TestingActionCall.ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a failing event message, it should signal an error of ApplyFailed and be Corrupted`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a FaillingEvent message is loaded
    let state = receive store actions state1 (InputMessage.loadEvent FailingEvent request)
    // Then the state should be the same but corrupted
    do state |> should equal { state1 with mode = Corrupted }
    // And a loading failed action should be registered
    do actions.calls |> should equal [ TestingActionCall.ErrorCall ApplyFailed ]

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadError message, it should signal an error of LoadingFailed and be Corrupted`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the LoadError message is loaded
    let state = receive store actions state1 (InputMessage.loadError (Exception("with message")))
    // Then the state should be the same but Corrupted
    do state |> should equal { state1 with mode = Corrupted }
    // And a loading failed action should be registered
    do actions.calls |> should equal [ TestingActionCall.ErrorCall LoadingFailed ]

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a LoadDone message, it should start Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the LoadDone message is loaded
    let state = receive store actions state1 (InputMessage.loadDone)
    // Then the state should be the same but Receiving
    do state |> should equal { state1 with mode = Receiving }
    // And no action should be received
    do actions.calls |> should equal []

[<Test>]
let ``When Aggregate behavior, while Loading, if it ReceiveCommand message, it should postpone the message and should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the ReceiveCommand message is loaded
    let state = receive store actions state1 (InputMessage.receiveCommand Increment (Some 4) request)
    // Then the state should be the same
    do state |> should equal state1
    // And a postpone action should be registered
    do actions.calls |> should equal [ PostponeCall ]

[<Test>]
let ``When Aggregate behavior, while Loading, if it receives a ApplyEvents message, it should signal an error of MessageRejected and should continue Loading`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And the ApplyEvents message is loaded
    let state = receive store actions state1 (InputMessage.applyEvents [ Incremented ] request)
    // Then the state should be the same
    do state |> should equal state1
    // And a postpone action should be registered
    do actions.calls |> should equal [ ErrorCall MessageRejected ]


(*  Receiving  *)

[<Test>]
let ``When Aggregate behavior, while Receiving from version 0, if it receives a valid command for version 0, it should emit an event and wait for confirmation of Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // Then the state should be the same but Emitting
    do state |> should equal { state1 with mode = Emitting }
    // And a emit action should be registered
    do actions.calls |> should equal [ EmitCall ([ Incremented ], request) ]

[<Test>]
let ``When Aggregate behavior, while Receiving from version 0, if it receives a valid command for any version, it should emit an event and wait for confirmation of Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for any version
    let state = receive store actions state1 (InputMessage.receiveCommand Increment None request)
    // Then the state should be the same but Emitting
    do state |> should equal { state1 with mode = Emitting }
    // And a emit action should be registered
    do actions.calls |> should equal [ EmitCall ([ Incremented ], request) ]

[<Test>]
let ``When Aggregate behavior, while Receiving from version 0, if it receives a valid command for version 1, it should signal an error of UnexpectedVersion and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 1
    let state = receive store actions state1 (InputMessage.receiveCommand Increment (Some 1) request)
    // Then the state should be the same but Emitting
    do state |> should equal state1
    // And an error action should be registered with UnexpectedVersion
    do actions.calls |> should equal [ ErrorCall UnexpectedVersion ]

[<Test>]
let ``When Aggregate behavior, while Receiving from version 4, if it receives a valid command, it should emit an event and wait for confirmation of Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 4
    let state = receive store actions state2 (InputMessage.receiveCommand Increment (Some 4) request)
    // Then the state should be the same but Emitting
    do state |> should equal { state2 with mode = Emitting }
    // And a emit action should be registered
    do actions.calls |> should equal [ EmitCall ([ Incremented ], request) ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives an invalid command, it should reject the command and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message with invalid command is loaded
    let state = receive store actions state2 (InputMessage.receiveCommand InvalidCommand None request)
    // Then the state should be the same
    do state |> should equal state2
    // And a reject action should be registered
    do actions.calls |> should equal [ RejectCall ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a validate-failing command, it should signal an error of ValidateFailed and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message with failing command is loaded
    let state = receive store actions state2 (InputMessage.receiveCommand ValidateFailCommand None request)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with ActFailed
    do actions.calls |> should equal [ ErrorCall ValidateFailed ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a failing command, it should signal an error of ActFailed and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message with failing command is loaded
    let state = receive store actions state2 (InputMessage.receiveCommand FailingCommand None request)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with ActFailed
    do actions.calls |> should equal [ ErrorCall ActFailed ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a do-not-act command, it should reject the command and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message with failing command is loaded
    let state = receive store actions state2 (InputMessage.receiveCommand DoNotActCommand None request)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with ActFailed
    do actions.calls |> should equal [ RejectCall ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a LoadEvent message, it should signal an error of MessageRejected and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.loadEvent Incremented request)
    // Then the state should be the same
    do state |> should equal state1
    // And an error action should be registered woth MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a LoadState message, it should signal an error of MessageRejected and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.loadState { incCount = 0; decCount = 0 } 0)
    // Then the state should be the same
    do state |> should equal state1
    // And an error action should be registered woth MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a LoadError message, it should signal an error of MessageRejected and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.loadError (Exception "An exception"))
    // Then the state should be the same
    do state |> should equal state1
    // And an error action should be registered woth MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a LoadDone message, it should signal an error of MessageRejected and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.loadDone)
    // Then the state should be the same
    do state |> should equal state1
    // And an error action should be registered woth MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Receiving, if it receives a ApplyEvents message, it should signal an error of MessageRejected and should continue Receiving`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // When a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state = receive store actions state1 (InputMessage.applyEvents [] request)
    // Then the state should be the same
    do state |> should equal state1
    // And an error action should be registered woth MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]


(*  Emitting  *)

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a ApplyEvents with valid events message, it should apply the events and should continue Receiving again`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded
    let state3 = receive store actions state2 (InputMessage.receiveCommand Increment None request)
    // When a ApplyEvents message with valid events is loaded
    do actions.clear ()
    let state = receive store actions state3 (InputMessage.applyEvents [ Incremented; Decremented ] request)
    // Then the state should be { Receiving, ver = 6, {+4,-2} }
    let expectedState = testingState Receiving 6 (events @ [ Incremented; Decremented ])
    do state |> should equal expectedState
    // And no action should be registered
    do actions.calls |> should equal [ RecoverCall ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a ApplyEvents with failing events message, it should signal an error with ApplyFailed and be Corrupted`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a snapshot {+3,-1} to load
    let events = [ Incremented; Incremented; Decremented; Incremented ]
    let snapshot = events |> foldBehavior
    // When the snapshot is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadState snapshot 4)
    // And a LoadDone message is loaded
    let state2 = receive store actions state1 (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded
    let state3 = receive store actions state2 (InputMessage.receiveCommand Increment None request)
    // When a ApplyEvents message with valid events is loaded
    do actions.clear ()
    let state = receive store actions state3 (InputMessage.applyEvents [ FailingEvent ] request)
    // Then the state should be the same but Corrupted
    do state |> should equal { state3 with mode = Corrupted }
    // And an error action should be registered with ApplyFailed
    do actions.calls |> should equal [ ErrorCall ApplyFailed ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a ReceiveCommand message, it should postpone the message and should continue Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state2 = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // When a LoadEvent message is loaded
    do actions.clear ()
    let state = receive store actions state2 (InputMessage.receiveCommand Increment None request)
    // Then the state should be the same
    do state |> should equal state2
    // And a postpone action should be registered
    do actions.calls |> should equal [ PostponeCall ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a LoadEvent message, it should signal an error of MessageRejected and should continue Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state2 = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // When a LoadEvent message is loaded
    do actions.clear ()
    let state = receive store actions state2 (InputMessage.loadEvent Increment request)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a LoadState message, it should signal an error of MessageRejected and should continue Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state2 = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // When a LoadEvent message is loaded
    do actions.clear ()
    let state = receive store actions state2 (InputMessage.loadState { incCount = 0; decCount = 0 } 0)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a LoadError message, it should signal an error of MessageRejected and should continue Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state2 = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // When a LoadEvent message is loaded
    do actions.clear ()
    let state = receive store actions state2 (InputMessage.loadError (Exception "An exception"))
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]

[<Test>]
let ``When Aggregate behavior, while Emitting, if it receives a LoadDone message, it should signal an error of MessageRejected and should continue Emitting`` () =
    // Given a store and actions
    let store, actions = newStoreReader [], newActions ()
    // And a LoadDone message is loaded
    let state1 = receive store actions (initBehavior()) (InputMessage.loadDone)
    // And a ReceiveCommand message is loaded for version 0
    let state2 = receive store actions state1 (InputMessage.receiveCommand Increment (Some 0) request)
    // When a LoadEvent message is loaded
    do actions.clear ()
    let state = receive store actions state2 (InputMessage.loadDone)
    // Then the state should be the same
    do state |> should equal state2
    // And an error action should be registered with MessageRejected
    do actions.calls |> should equal [ ErrorCall MessageRejected ]


(* readCurrentEvents *)

[<Test>]
let ``When an Aggregate is loaded and it has no stored events, then readCurrentEvents returns an empty commit observable`` () =
    // Given an empty store
    let store = newStoreReader []
    // When an aggregate is loaded from the store
    let messagesS = readCurrentEvents store request.aggregate
    // Then the stored messages collection is empty
    do messagesS.ToEnumerable() |> shouldBeSameSequence (seq [])

[<Test>]
let ``When an Aggregate is loaded and it has events stored, then readCurrentEvents returns the sequence of commited events`` () =
    // Given a couple of event commits
    let commits = [
        OnEventsCommit { events = [ Incremented; Incremented ]; prevVersion = -1; version = 2; request = request }
        OnEventsCommit { events = [ Decremented ]; prevVersion = 2; version = 3; request = request }
    ]
    // And a store with given events
    let store = newStoreReader commits
    // When an aggregate is loaded from the store
    let messagesS = readCurrentEvents store request.aggregate
    // Then the stored messages collection has the corresponding events
    do messagesS.ToEnumerable() 
        |> shouldBeSameSequence 
            (seq [
                InputMessage.loadEvent Incremented request
                InputMessage.loadEvent Incremented request
                InputMessage.loadEvent Decremented request
            ])
    
[<Test>]
let ``When an Aggregate is loaded and it has events + snapshot + events stored, then readCurrentEvents returns the sequence of commited snapshot + events`` () =
    // Given a couple of event + snapshot commits
    let commits = [
        //OnEventsCommit { events = [ Incremented; Incremented ]; prevVersion = -1; version = 2; request = request }
        OnSnapshotCommit { state = { incCount = 2; decCount = 0 }; version = 2 }
        OnEventsCommit { events = [ Decremented; Incremented ]; prevVersion = 2; version = 4; request = request }
    ]
    // And a store with given events
    let store = newStoreReader commits
    // When an aggregate is loaded from the store
    let messagesS = readCurrentEvents store request.aggregate
    // Then the stored messages collection has the corresponding snapshot + events
    do messagesS.ToEnumerable() 
        |> shouldBeSameSequence 
            (seq [
                InputMessage.loadState { incCount = 2; decCount = 0 } 2
                InputMessage.loadEvent Decremented request
                InputMessage.loadEvent Incremented request
            ])
