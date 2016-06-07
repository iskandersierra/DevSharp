module DevSharp.Server.Domain.Tests.ModuleAggregateClassCounterTest

open NUnit.Framework
open FsUnit
open DevSharp
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.Counter
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
let ``loading a ModuleAggregateClass with Counter aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``a class name of a Counter ModuleAggregateClass should be Samples.Domains.Counter`` () =
    aggregateClass.className
    |> should equal "Samples.Domains.Counter"

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
    |> fromSeqOptToListOpt<Event>
    |> should equal (Some [ WasIncremented ])

[<Test>] 
let ``applying a WasDecremented event over any state should return the initial state`` () =
    aggregateClass.apply WasDecremented init request
    |> should equal -1

[<Test>] 
let ``applying a WasIncremented event over any state should return the initial state`` () =
    aggregateClass.apply WasIncremented init request
    |> should equal 1
