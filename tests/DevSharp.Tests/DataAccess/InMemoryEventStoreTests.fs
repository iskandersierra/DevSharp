module ``In-memory event store tests``


open FSharp.Core
open System
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open FsUnit
open NUnit.Framework
open NUnit.Framework.Constraints

open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.TodoList


let initTitle = "TodoList initial title"
let title = "TodoList new title"
let tenantId : obj = "my tenant id" :> obj
let aggregateType : obj = "my aggregate type" :> obj
let aggregateId : obj = "my aggregate id" :> obj
let key = tenantId, aggregateType, aggregateId

let properties = 
    Map.empty
        .Add(AggregateIdConstant,      aggregateId)
        .Add(AggregateVersionConstant, 12345 :> obj)
        .Add(ApplicationIdConstant,    "my application id" :> obj)
        .Add(AggregateTypeConstant,    aggregateType)
        .Add(ProjectIdConstant,        "my project id" :> obj)
        .Add(CommandIdConstant,        "my command id" :> obj)
        .Add(CommandTypeConstant,      "my command type" :> obj)
        .Add(SessionIdConstant,        "my session id" :> obj)
        .Add(TenantIdConstant,         tenantId)
        .Add(UserIdConstant,           "my user id" :> obj)
        .Add(ClientDateConstant,       RequestDate.Now :> obj)
        .Add(ApiDateConstant,          RequestDate.Now :> obj)
        .Add(ProcessDateConstant,      RequestDate.Now :> obj)
let request = (toCommandRequest properties).Value

let toEventList list =
    list |> List.map (fun e -> e :> obj)

let trueOnCompleted () = true
let falseOnError exn = false

let writeCommitHelper (request: CommandRequest) (events: EventType list) (state: StateType option) (version: AggregateVersion option) (store: InMemoryEventStore) =
    let events = events |> toEventList
    let input = WriteCommitInput.create request events state version
    use waitHandle = new AutoResetEvent(false)
    use write = 
        (store.writeCommit input).Subscribe(
            Common.NoOp1,
            failWithExn "Observable should complete but failed with: ",
            resetAutoResetEvent waitHandle)
    do waitHandle.WaitOne(500) |> should be True
    

[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

[<Test>] 
let ``A new in-memory event store must be empty`` () =
    let store = InMemoryEventStore()
    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList []


[<Test>] 
let ``A new in-memory event store must accept an event commit with one event`` () =
    let store = InMemoryEventStore()
    let events: EventType list = [ WasCreated initTitle ]
    do writeCommitHelper request events None None store

    let commit = 
        OnEventsCommit { 
            events = events
            prevVersion = 0
            version = 1
            request = request 
        }
    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList [commit]

[<Test>] 
let ``A new in-memory event store must accept an event commit with one event and a snapshot`` () =
    let store = InMemoryEventStore()
    let events: EventType list = [ WasCreated initTitle ]
    let state = { title = initTitle; nextTaskId = 0; tasks = [] } :> StateType
    do writeCommitHelper request events (Some state) None store

    let commit = 
        OnSnapshotCommit { 
            state = state
            version = 1
        }
    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList [commit]

[<Test>] 
let ``A new in-memory event store must accept three event commits with snapshots and events`` () =
    let store = InMemoryEventStore()
    let events1: EventType list = [ WasCreated initTitle; TitleWasUpdated title ]
    let events2: EventType list = [ TaskWasAdded (1, "task #1"); TaskWasAdded (2, "task #2") ]
    let events3: EventType list = [ TaskWasUpdated (1, "new task #1"); TaskWasChecked 1 ]
    let state2 = 
        { 
            title = title
            nextTaskId = 0
            tasks = 
            [
                { id = 1; text = "task #1"; isChecked = false }
                { id = 2; text = "task #2"; isChecked = false }
            ] 
        } :> StateType
    do writeCommitHelper request events1 None None store
    do writeCommitHelper request events2 (Some state2) None store
    do writeCommitHelper request events3 None None store

    let commit2 = 
        OnSnapshotCommit { 
            state = state2
            version = 4
        }
    let commit3 = 
        OnEventsCommit { 
            events = events3
            prevVersion = 4
            version = 6
            request = request 
        }

    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList [commit2; commit3]

