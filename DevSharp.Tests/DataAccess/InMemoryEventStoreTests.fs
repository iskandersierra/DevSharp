module ``In-memory event store tests``


open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.TodoList
open DevSharp
open DevSharp.DataAccess
open NUnit.Framework.Constraints


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
    do store.getAllAggregates () |> should equal Map.empty

[<Test>] 
let ``A new in-memory event store must accept an event commit with one event`` () =
    let store = InMemoryEventStore()
    let events = [ WasCreated initTitle ] |> toEventList
    let task = store.writeCommit (completion trueOnCompleted falseOnError) (WriteCommitInput.create request events None EventStoreCommit.newVersion)
    let commit = { 
        events = events
        prevVersion = EventStoreCommit.newVersion
        version = EventStoreCommit.newVersion + 1
        request = request 
    }
    do task |> should not' (be Null)
    do task.Result |> should be True
    do store.getAllAggregates () 
        |> should equal Map.empty

