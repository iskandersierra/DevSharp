module DevSharp.Server.Domain.Tests.ModuleAggregateClassTodoListTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.TodoList
open DevSharp.Messaging
open DevSharp.Server.ReflectionUtils
open NUnit.Framework.Constraints


let initTitle = "TodoList initial title"
let title = "TodoList new title"

let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = CommandRequest(new Map<string, obj>(seq []))
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