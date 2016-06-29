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
    let events = [ WasCreated initTitle ] |> toEventList
    let input = WriteCommitInput.create request events None EventStoreCommit.newVersion
    use waitHandle = new AutoResetEvent(false)
    use write = 
        (store.writeCommit input).Subscribe(
            Common.NoOp1,
            failWithExn "Observable should complete but failed with: ",
            resetAutoResetEvent waitHandle)
    do waitHandle.WaitOne(500) |> should be True

    let commit = 
        OnEventsCommit { 
            events = events
            prevVersion = EventStoreCommit.newVersion
            version = EventStoreCommit.newVersion + 1
            request = request 
        }
    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList [commit]

[<Test>] 
let ``A new in-memory event store must accept an event commit with one event and a snapshot`` () =
    let store = InMemoryEventStore()
    let events = [ WasCreated initTitle ] |> toEventList
    let state = { title = initTitle; nextTaskId = 0; tasks = [] } :> StateType
    let input = WriteCommitInput.create request events (Some state) EventStoreCommit.newVersion
    use waitHandle = new AutoResetEvent(false)
    use write = 
        (store.writeCommit input).Subscribe(
            Common.NoOp1,
            failWithExn "Observable should complete but failed with: ",
            resetAutoResetEvent waitHandle)
    do waitHandle.WaitOne(500) |> should be True

    let commit = 
        OnSnapshotCommit { 
            state = state
            version = EventStoreCommit.newVersion + 1
        }
    let input = ReadCommitsInput.create request.aggregate
    do store.readCommits input |> shouldProduceList [commit]

