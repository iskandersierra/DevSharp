open System
open System.Threading.Tasks
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open FSharp.Core
open Akka
open Akka.Actor
open Akka.Configuration

open DevSharp
open DevSharp.DataAccess
open DevSharp.Domain.Aggregates
open DevSharp.Server.Domain
open DevSharp.AkkaActors.AggregateActors
open Samples.Domains.TodoList

module TestInstanceActor =
    open DevSharp.Domain.Aggregates.AggregateBehavior

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
    let aggregateClass = ModuleAggregateClass(aggregateModuleType) :> IAggregateClass

    let toEventList list =
        list |> List.map (fun e -> e :> obj)

    let configSystem () =
        ConfigurationFactory.ParseString("
        akka {
            actor {
                serializers {
                  wire = \"Akka.Serialization.WireSerializer, Akka.Serialization.Wire\"
                }
                serialization-bindings {
                  \"System.Object\" = wire
                }
            }
            loggers = [\"Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog\"]
        }")

    let writeCommitHelper (request: CommandRequest) (events: EventType list) (state: StateType option) (version: AggregateVersion option) (store: InMemoryEventStore) =
        let events = events |> toEventList
        let input = WriteCommitInput.create request events state version
        use waitHandle = new AutoResetEvent(false)
        use write = 
            (store.writeCommit input).Subscribe(
                Common.NoOp1,
                failWithExn "Observable should complete but failed with: ",
                resetAutoResetEvent waitHandle)
        do waitHandle.WaitOne(500) |> ignore


    let ``An aggregate instance actor with no events and no receiving command should not send any messages`` () =
        let system = ActorSystem.Create ("test-system", configSystem()) in
        let store = InMemoryEventStore()
        let props = Props.Create<InstanceActor> (store , store, request.aggregate, aggregateClass)
                    
        let instance = system.ActorOf (props, "myTodoList")
        let askTask = instance.Ask (InputMessage.loadDone)
        let task = askTask.ContinueWith(fun (t: Task<obj>) -> printfn "Response message: %A" t.Result)
        do task.Wait (1000) |> ignore
        do printfn "press any key to continue ..."
        do Console.ReadKey() |> ignore
        do system.Terminate().Wait()

[<EntryPoint>]
let main argv = 
    do TestInstanceActor.``An aggregate instance actor with no events and no receiving command should not send any messages`` ()
//    do printfn "press any key to continue ..."
//    do Console.ReadKey() |> ignore
    0 // return an integer exit code
