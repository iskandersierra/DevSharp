module DevSharp.Server.Domain.Tests.ModuleAggregateClassEchoerTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.Echoer
open DevSharp
open NUnit.Framework.Constraints
open DevSharp.Server.ReflectionUtils


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
let aggregateModuleType = typedefof<Command>.DeclaringType
let mutable aggregateClass = NopAggregateClass() :> IAggregateClass
let request = (toCommandRequest properties).Value
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
let ``a class name of a Echoer ModuleAggregateClass should be Samples.Domains.Echoer`` () =
    aggregateClass.className
    |> should equal "Samples.Domains.Echoer"

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