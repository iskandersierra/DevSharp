module ``Command request tests``

open NUnit.Framework
open FsUnit
open DevSharp
open DevSharp.Messaging
open NUnit.Framework.Constraints



[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))


[<Test>]
let ``An empty command request should have all its parameters as None`` () =
    let properties = Map.empty

    let onFound x = true
    let onNotFound () = false

    let request = CommandRequest properties
    request |> should not' (be Null)
    request.properties       |> should equal properties
    request.aggregateId      |> should equal None
    request.aggregateVersion |> should equal None
    request.applicationId    |> should equal None
    request.projectId        |> should equal None
    request.sessionId        |> should equal None
    request.tenantId         |> should equal None
    request.userId           |> should equal None

    request.getAggregateId      onFound onNotFound |> should equal false
    request.getAggregateVersion onFound onNotFound |> should equal false
    request.getApplicationId    onFound onNotFound |> should equal false
    request.getProjectId        onFound onNotFound |> should equal false
    request.getSessionId        onFound onNotFound |> should equal false
    request.getTenantId         onFound onNotFound |> should equal false
    request.getUserId           onFound onNotFound |> should equal false

[<Test>]
let ``An fully loaded command request should have all its parameters as Some value`` () =
    let properties = 
        Map.empty
            .Add(AggregateIdConstant,      "my aggregate id" :> obj)
            .Add(AggregateVersionConstant, 12345 :> obj)
            .Add(ApplicationIdConstant,    "my application id" :> obj)
            .Add(ProjectIdConstant,        "my project id" :> obj)
            .Add(SessionIdConstant,        "my session id" :> obj)
            .Add(TenantIdConstant,         "my tenant id" :> obj)
            .Add(UserIdConstant,           "my user id" :> obj)

    let onFound x = true
    let onNotFound () = false

    let request = CommandRequest properties
    request |> should not' (be Null)
    request.properties       |> should equal properties
    request.aggregateId      |> should equal (Some "my aggregate id")
    request.aggregateVersion |> should equal (Some 12345)
    request.applicationId    |> should equal (Some "my application id")
    request.projectId        |> should equal (Some "my project id")
    request.sessionId        |> should equal (Some "my session id")
    request.tenantId         |> should equal (Some "my tenant id")
    request.userId           |> should equal (Some "my user id")

    request.getAggregateId      onFound onNotFound |> should equal true
    request.getAggregateVersion onFound onNotFound |> should equal true
    request.getApplicationId    onFound onNotFound |> should equal true
    request.getProjectId        onFound onNotFound |> should equal true
    request.getSessionId        onFound onNotFound |> should equal true
    request.getTenantId         onFound onNotFound |> should equal true
    request.getUserId           onFound onNotFound |> should equal true
