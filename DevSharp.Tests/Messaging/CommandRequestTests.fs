module ``Command request tests``

open NUnit.Framework
open System
open FsUnit
open DevSharp
open NUnit.Framework.Constraints

let date1 = RequestDate.Now
let date2 = date1.AddMinutes(1.) 
let date3 = date1.AddMinutes(2.) 

[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))


[<Test>]
let ``An empty command request should be equal to None`` () =
    let properties = Map.empty

    let request = toCommandRequest properties
    request |> should equal None

[<Test>]
let ``A fully loaded command request should have all its parameters with expected values`` () =
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
            .Add(ClientDateConstant,       date1 :> obj)
            .Add(ApiDateConstant,          date2 :> obj)
            .Add(ProcessDateConstant,      date3 :> obj)

    let request = toCommandRequest properties
    request |> should not' (be Null)
    match request with
    | Some req ->
        req.aggregateVersion |> should equal 12345
        req.commandId |> should equal "my command id"
        req.processDate |> should equal date3
        req.commandId |> should equal "my command id"
        req.commandType |> should equal "my command type"
        req.aggregate.aggregateId |> should equal "my aggregate id"
        req.aggregate.aggregateType |> should equal "my aggregate type"
        req.aggregate.request.properties |> should equal properties
        req.aggregate.request.sessionId |> should equal "my session id"
        req.aggregate.request.tenantId |> should equal "my tenant id"
        req.aggregate.request.userId |> should equal "my user id"
        req.aggregate.request.clientDate |> should equal date1
        req.aggregate.request.apiDate |> should equal date2
