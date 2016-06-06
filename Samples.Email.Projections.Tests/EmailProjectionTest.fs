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


[<Test>]
let ``EmailProjection fullname should be Samples.EmailProjection`` () =
    moduleType.FullName 
    |> should equal "Samples.EmailProjection"

[<Test>]
let ``Update subject should produce the same email with the new subject`` () =
    update 
        { 
            id=Guid.Empty
            subject="" 
            from="a@aa.com" 
            toEmails=["b@bb.com"] 
            ccEmails=[] 
            bccEmails =[] 
            body="..." 
        } (SubjectWasUpdated (Guid.Empty, "New Subject")) (Request(Unchecked.defaultof<Map<string,obj>>)) 
    |> should equal 
        { 
            id=Guid.Empty
            subject="New Subject"
            from="a@aa.com"
            toEmails=["b@bb.com"]
            ccEmails=[] 
            bccEmails =[]
            body="..."
        }


