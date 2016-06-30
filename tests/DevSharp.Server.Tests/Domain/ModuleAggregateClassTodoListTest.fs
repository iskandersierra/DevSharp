module DevSharp.Server.Domain.Tests.ModuleAggregateClassTodoListTest

open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit

open DevSharp
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open DevSharp.Server.ReflectionUtils
open Samples.Domains.TodoList


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
let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let createdState() = apply (WasCreated title) init

[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

[<Test>] 
let ``loading a ModuleAggregateClass with TodoList aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``a class name of a TodoList ModuleAggregateClass should be Samples.Domains.TodoList`` () =
    aggregateClass.className
    |> should equal "Samples.Domains.TodoList"

[<Test>] 
let ``validating a correct Create command should give a valid result`` () =
    (aggregateClass.validate (Create initTitle) request).isValid
    |> should be True

[<Test>] 
let ``validating an incorrect Create command should give a valid result`` () =
    (aggregateClass.validate (Create <| TodoListTitle null) request).isValid
    |> should be False

[<Test>] 
let ``acting with a Create command over initial state should return a WasCreated event`` () =
    aggregateClass.act (Create initTitle) init request
    |> fromSeqOptToListOpt<Event>
    |> should equal (Some [ WasCreated initTitle ])

[<Test>] 
let ``acting with a Create command over some state should return None`` () =
    aggregateClass.act (Create initTitle) (createdState()) request
    |> fromSeqOptToListOpt<Event>
    |> should be Null

[<Test>] 
let ``acting with a UpdateTitle command over initial state should return None`` () =
    aggregateClass.act (UpdateTitle initTitle) init request 
    |> fromSeqOptToListOpt<Event>
    |> should be Null

[<Test>] 
let ``acting with a Create command over some state should return a WasCreated event`` () =
    aggregateClass.act (UpdateTitle initTitle) (createdState()) request
    |> fromSeqOptToListOpt<Event>
    |> should equal (Some [ TitleWasUpdated initTitle ])

[<Test>] 
let ``applying a WasCreated event over initial state should return the some state`` () =
    aggregateClass.apply (WasCreated initTitle) init request
    |> should equal (apply (TitleWasUpdated initTitle) (createdState()))