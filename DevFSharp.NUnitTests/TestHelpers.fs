[<AutoOpen>]
module DevFSharp.NUnitTests.TestHelpers

open System
open FSharp.Core
open DevFSharp.Validations
open NUnit.Framework

let testIsValidCommand validate command =
    let validation = validate command
    in
    Assert.That ( Seq.length validation, Is.EqualTo 0 )

let testIsInvalidCommand validate command =
    let validation = validate command
    in
    Assert.That ( Seq.length validation, Is.Not.EqualTo 0 )


let testProcessCommandIsValid (processCommand: 'command -> 'state -> 'event list) (state: 'state) (command: 'command) (expectedEvents: 'event list) =
    let events = processCommand command state
    in
    Assert.That (events, Is.EquivalentTo expectedEvents )

let testProcessCommandIsInvalid (processCommand: 'command -> 'state -> 'event list) (state: 'state) (command: 'command) =
    let call = fun () -> processCommand command state |> ignore
    in
    Assert.That (call, Throws.TypeOf<MatchFailureException>())


let testReceiveEventIsValid (receiveEvent: 'event -> 'state -> 'state) (state: 'state) (event: 'event) (expectedState: 'state) =
    let state2 = receiveEvent event state
    in
    Assert.That (state2, Is.EqualTo expectedState )

let testReceiveEventIsInvalid (receiveEvent: 'event -> 'state -> 'state) (state: 'state) (event: 'event) =
    let call = fun () -> receiveEvent event state |> ignore
    in
    Assert.That (call, Throws.TypeOf<MatchFailureException>())

