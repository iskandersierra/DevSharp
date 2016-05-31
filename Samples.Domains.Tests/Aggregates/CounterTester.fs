module ``Counter tests``

open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.Counter


let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType

// Definition

[<Test>]
let ``Counter fullname should be Samples.Domains.Counter`` () =
    moduleType.FullName 
    |> should equal "Samples.Domains.Counter"

[<Test>]
let ``Counter Event should be defined as expected`` () =
    eventType 
    |> shouldBeAUnion
        { 
            unionName = "Event";
            cases = 
            [
                { caseName = "WasIncremented";  types = [ ] }; 
                { caseName = "WasDecremented";  types = [ ] }; 
            ]
        }

[<Test>]
let ``Counter Command should be defined as expected`` () =
    commandType 
    |> shouldBeAUnion
        { 
            unionName = "Command";
            cases = 
            [
                { caseName = "Increment";  types = [ ] }; 
                { caseName = "Decrement";  types = [ ] }; 
            ]
        }


// Runtime

[<Test>]
let ``Counter initial state should be zero`` () =
    init
    |> should equal 0

[<Test>]
let ``Counter acting on Increment command over initial state gives WasIncremented`` () =
    act Increment
    |> should equal [ WasIncremented ]

[<Test>]
let ``Counter acting on Decrement command over initial state gives WasDecremented`` () =
    act Decrement
    |> should equal [ WasDecremented ]
