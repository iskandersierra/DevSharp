namespace DevSharp.AkkaActors.Tests.Aggregates

open System
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open FSharp.Core
open FsUnit
open NUnit.Framework
open NUnit.Framework.Constraints
open Akka.Actor
open Akka.TestKit.NUnit3

open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open Samples.Domains.TodoList
open DevSharp.AkkaActors.AggregateActors

[<TestFixture>]
type InstanceActorTests () =
    inherit TestKit()

    let initTitle = "TodoList initial title"
    let title = "TodoList new title"
    let tenantId : obj = "my tenant id" :> obj
    let aggregateType : obj = "my aggregate type" :> obj
    let aggregateId : obj = "my aggregate id" :> obj
    let key = tenantId, aggregateType, aggregateId

    let properties = 
        Map.empty
            .Add(AggregateIdConstant,      aggregateId)
            .Add(AggregateVersionConstant, 12345 :> obj)
            .Add(ApplicationIdConstant,    "my application id" :> obj)
            .Add(AggregateTypeConstant,    aggregateType)
            .Add(ProjectIdConstant,        "my project id" :> obj)
            .Add(CommandIdConstant,        "my command id" :> obj)
            .Add(CommandTypeConstant,      "my command type" :> obj)
            .Add(SessionIdConstant,        "my session id" :> obj)
            .Add(TenantIdConstant,         tenantId)
            .Add(UserIdConstant,           "my user id" :> obj)
            .Add(ClientDateConstant,       RequestDate.Now :> obj)
            .Add(ApiDateConstant,          RequestDate.Now :> obj)
            .Add(ProcessDateConstant,      RequestDate.Now :> obj)
    let request = (toCommandRequest properties).Value
    let aggregateModuleType = typedefof<Command>.DeclaringType
    let mutable aggregateClass = NopAggregateClass() :> IAggregateClass

    let toEventList list =
        list |> List.map (fun e -> e :> obj)

    let writeCommitHelper (request: CommandRequest) (events: EventType list) (state: StateType option) (version: AggregateVersion option) (store: InMemoryEventStore) =
        let events = events |> toEventList
        let input = WriteCommitInput.create request events state version
        use waitHandle = new AutoResetEvent(false)
        use write = 
            (store.writeCommit input).Subscribe(
                Common.NoOp1,
                failWithExn "Observable should complete but failed with: ",
                resetAutoResetEvent waitHandle)
        do waitHandle.WaitOne(500) |> should be True

    [<SetUp>]
    member __.testSetup () =
        aggregateClass <- ModuleAggregateClass(aggregateModuleType)
        TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

    [<Test>] 
    member this.``An aggregate instance actor with no events and no receiving command should not send any messages`` () =
        let store = InMemoryEventStore()
        let instance = (typedefof<InstanceActor>, store, store, aggregateClass, aggregateId)
                    |> Props.Create
                    |> this.Sys.ActorOf
        do this.ExpectNoMsg 50
        do Assert.Fail ""
