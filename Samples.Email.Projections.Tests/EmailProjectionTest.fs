module ``EmailProjection tests``

open System
open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Messaging
open DevSharp.Testing.DomainTesting
open Samples.Email.Logic
open Samples.EmailProjection


let instanceType   = typedefof<Instance>
let moduleType  = instanceType.DeclaringType
let request = Request(Map.empty<string,obj>)


[<Test>]
let ``EmailProjection fullname should be Samples.EmailProjection`` () =
    moduleType.FullName 
    |> should equal "Samples.EmailProjection"

[<Test>]
let ``Update subject should produce the same email with the new subject`` () =
    let email = { 
            id        = Guid.Empty
            subject   = "" 
            from      = "a@aa.com" 
            toEmails  = [ "b@bb.com" ] 
            ccEmails  = [ ] 
            bccEmails = [ ] 
            body      = "..." 
        }
    let event = SubjectWasUpdated (Guid.Empty, "New Subject")
    let expected = { email with subject = "New Subject" }
    let actual = update email event request 
    actual |> should equal (Some expected)


