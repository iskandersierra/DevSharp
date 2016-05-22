[<AutoOpen>]
module DevFSharp.NUnitTests.TestHelpers

open System
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


let testProcessCommandIsValid (processCommand: 'state option -> 'command -> 'event list) (state: 'state option) (command: 'command) (expectedEvents: 'event list) =
    let events = processCommand state command
    in
    Assert.That (events, Is.EquivalentTo expectedEvents )

let testProcessCommandIsInvalid (processCommand: 'state option -> 'command -> 'event list) (state: 'state option) (command: 'command) =
    let call = fun () -> processCommand state command |> ignore
    in
    Assert.That (call, Throws.TypeOf<NotSupportedException>())
    

let testReceiveEventIsValid (receiveEvent: 'state option -> 'event -> 'state option) (state: 'state option) (event: 'event) (expectedState: 'state option) =
    let state2 = receiveEvent state event
    in
    Assert.That (state2, Is.EqualTo expectedState )

let testReceiveEventIsInvalid (receiveEvent: 'state option -> 'event -> 'state option) (state: 'state option) (event: 'event) =
    let call = fun () -> receiveEvent state event |> ignore
    in
    Assert.That (call, Throws.TypeOf<NotSupportedException>())

