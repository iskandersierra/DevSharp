module DevSharp.Server.Domain.Tests.ModuleAggregateClassPingPongTest

open NUnit.Framework
open FsUnit
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.PingPong
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
let ``loading a ModuleAggregateClass with PingPong aggregate module definition do not fail`` () =
    aggregateClass 
    |> should not' (be Null)

[<Test>] 
let ``a class name of a PingPong ModuleAggregateClass should be Samples.Domains.PingPong`` () =
    aggregateClass.className
    |> should equal "Samples.Domains.PingPong"

[<Test>] 
let ``validating a Ping command should give a valid result`` () =
    (aggregateClass.validate Ping request).isValid
    |> should be True

[<Test>] 
let ``validating a Pong command should give a valid result`` () =
    (aggregateClass.validate Pong request).isValid
    |> should be True

[<Test>] 
let ``acting with a Ping command over some state should return a WasPinged event`` () =
    aggregateClass.act Ping null request
    |> fromSeqOptToListOpt<Event>
    |> should equal (Some [ WasPinged ])

[<Test>] 
let ``applying a WasPonged event over any state should return the initial state`` () =
    aggregateClass.apply WasPonged null request
    |> should equal null