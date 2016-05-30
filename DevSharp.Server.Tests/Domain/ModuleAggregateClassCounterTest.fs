module DevSharp.Server.Domain.Tests.ModuleAggregateClassCounterTest

open NUnit.Framework
open FsUnit
open DevSharp.Messaging
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.Counter


let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = Request(new Map<string, obj>(seq []))
let message = "Helloooo!"

[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)

[<Test>] 
let ``loading a ModuleAggregateClass with Counter aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``validating a Increment command should give a valid result`` () =
    (aggregateClass.validate Increment request).isValid
    |> should be True

[<Test>] 
let ``validating a Decrement command should give a valid result`` () =
    (aggregateClass.validate Decrement request).isValid
    |> should be True

[<Test>] 
let ``acting with a Increment command over some state should return a WasEchod event`` () =
    aggregateClass.act Increment init request
    |> should equal [ WasIncremented ]

[<Test>] 
let ``applying a WasDecremented event over any state should return the initial state`` () =
    aggregateClass.apply WasDecremented init request
    |> should equal -1

[<Test>] 
let ``applying a WasIncremented event over any state should return the initial state`` () =
    aggregateClass.apply WasIncremented init request
    |> should equal 1
