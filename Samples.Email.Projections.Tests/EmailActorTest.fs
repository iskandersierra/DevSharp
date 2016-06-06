module ``EmailActor tests``

open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Email.EmailActor


let greetType   = typedefof<Greet>
let moduleType  = greetType.DeclaringType


[<Test>]
let ``EmailActor fullname should be Samples.Email.EmailActor`` () =
    moduleType.FullName 
    |> should equal "Samples.Email.EmailActor"