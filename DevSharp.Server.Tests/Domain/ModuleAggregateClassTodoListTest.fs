module DevSharp.Server.Domain.Tests.ModuleAggregateClassTodoListTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.TodoList
open DevSharp.Messaging


let initTitle = TodoListTitle "TodoList initial title"
let title = TodoListTitle "TodoList new title"

let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = Request(new Map<string, obj>(seq []))
let createdState() = apply (WasCreated title) init

[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)

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
    |> should equal [ WasCreated initTitle ]

[<Test>] 
let ``acting with a Create command over some state should fail`` () =
    (fun () -> aggregateClass.act (Create initTitle) (createdState()) request |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>] 
let ``acting with a UpdateTitle command over initial state should fail`` () =
    (fun () -> aggregateClass.act (UpdateTitle initTitle) init request |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>] 
let ``acting with a Create command over some state should return a WasCreated event`` () =
    aggregateClass.act (UpdateTitle initTitle) (createdState()) request
    |> should equal [ TitleWasUpdated initTitle ]

[<Test>] 
let ``applying a WasCreated event over initial state should return the some state`` () =
    aggregateClass.apply (WasCreated initTitle) init request
    |> should equal (apply (TitleWasUpdated initTitle) (createdState()))