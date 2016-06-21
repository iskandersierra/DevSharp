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
let request = (toCommandRequest properties).Value

let toEventList list =
    list |> List.map (fun e -> e :> obj)

[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

[<Test>] 
let ``A new in-memory event store must be empty`` () =
    let store = InMemoryEventStore()
    do store.getAllEvents () |> should equal []
    do store.getAllSnapshots () |> should equal Map.empty

[<Test>] 
let ``A new in-memory event store must accept an event commit with one event`` () =
    let store = InMemoryEventStore()
    let events = [ WasCreated initTitle ] |> toEventList
    let task = store.WriteCommit (fun () -> true) (fun exn -> false) request events None EventStoreCommit.newVersion
    let commit = { 
        events = events
        prevVersion = EventStoreCommit.newVersion
        lastVersion = 1
        request = request 
    }
    do task |> should not' (be Null)
    do task.Result |> should be True
    do store.getAllEvents () |> should equal [ commit ]
    do store.getAllSnapshots () |> should equal Map.empty

