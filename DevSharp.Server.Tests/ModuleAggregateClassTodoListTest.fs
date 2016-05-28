module DevSharp.Server.Tests.ModuleAggregateClassTodoListTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain
open DevSharp.Server.Domain
open Samples.Domains.TodoList
open DevSharp.Messaging


let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = Request(new Map<string, obj>(seq []))
let message = "Helloooo!"
let createdState() = apply (WasCreated "Some title") init

[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)

[<Test>] 
let ``loading a ModuleAggregateClass with TodoList aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``validating a correct Create command should give a valid result`` () =
    (aggregateClass.validate (Create message) request).isValid
    |> should be True

[<Test>] 
let ``validating an incorrect Create command should give a valid result`` () =
    (aggregateClass.validate (Create null) request).isValid
    |> should be False

[<Test>] 
let ``acting with a Create command over initial state should return a WasCreated event`` () =
    aggregateClass.act (Create message) init request
    |> should equal [ WasCreated message ]

[<Test>] 
let ``acting with a Create command over some state should fail`` () =
    (fun () -> aggregateClass.act (Create message) (createdState()) request |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>] 
let ``acting with a UpdateTitle command over initial state should fail`` () =
    (fun () -> aggregateClass.act (UpdateTitle message) init request |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>] 
let ``acting with a Create command over some state should return a WasCreated event`` () =
    aggregateClass.act (UpdateTitle message) (createdState()) request
    |> should equal [ TitleWasUpdated message ]

[<Test>] 
let ``applying a WasCreated event over initial state should return the some state`` () =
    aggregateClass.apply (WasCreated message) init request
    |> should equal (apply (TitleWasUpdated message) (createdState()))