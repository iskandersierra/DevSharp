
module Samples.Email.EmailActor

open Akka.Actor
open Akka.FSharp

// Create an (immutable) message type that your actor will respond to
type Greet = Greet of string


let system = ActorSystem.Create "MySystem"

// Use F# computation expression with tail-recursive loop
// to create an actor message handler and return a reference
let greeter = spawn system "greeter" <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        match msg with
        | Greet who -> printf "Hello, %s!\n" who
        return! loop() }
    loop()

greeter <! Greet "World"
printfn "%s" greeter.Path.Address.System