module ``PingPong tests``

open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.PingPong
open NUnit.Framework.Constraints


let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType


[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))


// Definition

[<Test>]
let ``PingPong fullname should be Samples.Domains.PingPong`` () =
    moduleType.FullName 
    |> should equal "Samples.Domains.PingPong"

[<Test>]
let ``PingPong Event should be defined as expected`` () =
    eventType 
    |> shouldBeAUnion
        { 
            unionName = "Event";
            cases = 
            [
                { caseName = "WasPonged";  types = [ ] }; 
                { caseName = "WasPinged";  types = [ ] }; 
            ]
        }

[<Test>]
let ``PingPong Command should be defined as expected`` () =
    commandType 
    |> shouldBeAUnion
        { 
            unionName = "Command";
            cases = 
            [
                { caseName = "Ping";  types = [ ] }; 
                { caseName = "Pong";  types = [ ] }; 
            ]
        }


// Runtime

[<Test>]
let ``PingPong acting on Ping command over initial state gives WasPinged`` () =
    act Ping 
    |> should equal (Some [ WasPinged ])

[<Test>]
let ``PingPong acting on Pong command over initial state gives WasPonged`` () =
    act Pong 
    |> should equal (Some [ WasPonged ])
