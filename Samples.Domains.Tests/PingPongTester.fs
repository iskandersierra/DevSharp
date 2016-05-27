module ``PingPong tests``

open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.PingPong


let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType

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
let ``PingPong initial state should be null`` () =
    init
    |> should equal null

[<Test>]
let ``PingPong acting on Ping command over initial state gives WasPinged`` () =
    act Ping 
    |> should equal [ WasPinged ]

[<Test>]
let ``PingPong acting on Pong command over initial state gives WasPonged`` () =
    act Pong 
    |> should equal [ WasPonged ]
