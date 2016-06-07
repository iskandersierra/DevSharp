module DevSharp.Server.Domain.Tests.ModuleAggregateClassEchoerTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.Echoer
open DevSharp.Messaging
open NUnit.Framework.Constraints
open DevSharp.Server.ReflectionUtils


let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = CommandRequest(new Map<string, obj>(seq []))
let message = "Helloooo!"

[<SetUp>]
let testSetup () =
    aggregateClass <- ModuleAggregateClass(aggregateModuleType)
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

[<Test>] 
let ``loading a ModuleAggregateClass with Echoer aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``validating a correct Echo command should give a valid result`` () =
    (aggregateClass.validate (Echo message) request).isValid
    |> should be True

[<Test>] 
let ``validating a message-less Echo command should give a valid result`` () =
    (aggregateClass.validate (Echo null) request).isValid
    |> should be True

[<Test>] 
let ``acting with a Echo command over some state should return a WasEchod event`` () =
    aggregateClass.act (Echo message) init request
    |> fromSeqOptToListOpt<Event>
    |> should equal (Some [ WasEchoed message ])

[<Test>] 
let ``applying a WasEchoed event over any state should return the initial state`` () =
    aggregateClass.apply (WasEchoed message) init request
    |> should equal init