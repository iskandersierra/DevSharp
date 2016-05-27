module ``Echoer tests``

open System
open NUnit.Framework
open FsUnit
open FSharp.Reflection
open DevSharp.Validations
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains


let shortTextLength = 3
let longTextLength  = 3
let shortText       = String ('a', shortTextLength)
let longText        = String ('a', longTextLength)

let eventType   = typedefof<Echoer.Event>
let commandType = typedefof<Echoer.Command>
let moduleType  = commandType.DeclaringType

//let eventCases = FSharpType.GetUnionCases eventType |> Array.toList
//let commandCases = FSharpType.GetUnionCases commandType |> Array.toList
//let expectedEvents = [ ("WasEchoed", [ typedefof<string> ]) ]
//let expectedCommands = [ ("Echo", [ typedefof<string> ]) ]

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
            cases = [
                //{ caseName = "Nothing"; types = [ ] };
                { caseName = "WasEchoed";  types = [ typedefof<int>; typedefof<string> ] }; 
            ]
        }


// Runtime

[<Test>]
let ``initial state shoulf be null`` () =
    Echoer.init 
    |> should be Null

