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


let testProcessCommandIsValid (act: 'command -> 'state -> 'event list) (state: 'state) (command: 'command) (expectedEvents: 'event list) =
    let events = act command state
    in
    Assert.That (events, Is.EquivalentTo expectedEvents )

let testProcessCommandIsInvalid (act: 'command -> 'state -> 'event list) (state: 'state) (command: 'command) =
    let call = fun () -> act command state |> ignore
    in
    Assert.That (call, Throws.TypeOf<MatchFailureException>())


let testReceiveEventIsValid (apply: 'event -> 'state -> 'state) (state: 'state) (event: 'event) (expectedState: 'state) =
    let state2 = apply event state
    in
    Assert.That (state2, Is.EqualTo expectedState )

let testReceiveEventIsInvalid (apply: 'event -> 'state -> 'state) (state: 'state) (event: 'event) =
    let call = fun () -> apply event state |> ignore
    in
    Assert.That (call, Throws.TypeOf<MatchFailureException>())

