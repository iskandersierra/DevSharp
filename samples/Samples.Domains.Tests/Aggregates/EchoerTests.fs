module ``Echoer tests``

open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.Echoer
open NUnit.Framework.Constraints


let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType



[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))


// Definition

[<Test>]
let ``Echoer fullname should be Samples.Domains.Echoer`` () =
    moduleType.FullName 
    |> should equal "Samples.Domains.Echoer"

[<Test>]
let ``Echoer Event should be defined as expected`` () =
    eventType 
    |> shouldBeAUnion
        { 
            unionName = "Event";
            cases = 
            [
                { caseName = "WasEchoed";  types = [ typedefof<string> ] }; 
            ]
        }

[<Test>]
let ``Echoer Command should be defined as expected`` () =
    commandType 
    |> shouldBeAUnion
        { 
            unionName = "Command";
            cases = 
            [
                { caseName = "Echo";  types = [ typedefof<string> ] }; 
            ]
        }


// Runtime

let message = "Random message"

[<Test>]
let ``Echoer initial state should be null`` () =
    init
    |> should equal null

[<Test>]
let ``Echoer acting on Echo command with message over initial state gives WasEchoed with the same message`` () =
    act (Echo message) 
    |> should equal (Some [ WasEchoed message ])

