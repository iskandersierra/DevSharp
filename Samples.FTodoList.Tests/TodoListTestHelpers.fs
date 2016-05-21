[<AutoOpen>]
module Samples.FTodoList.Tests.TodoListTestHelpers

open System
open Samples.FTodoList.TodoList
open DevFSharp.Validations
open NUnit.Framework

let initialState : State option = None
let defaultTitle = "Title of todo list"
let emptyState = Some { title = defaultTitle; nextTaskId = 1; tasks = [] }
let emptyStateTitle title = Some { title = title; nextTaskId = 1; tasks = [] }
let createState checks = 
    { 
        title = defaultTitle; 
        nextTaskId = (List.length checks) + 1; 
        tasks = checks 
            |> List.mapi (fun i isChecked -> 
                { 
                    id = i + 1; 
                    text = "task #" + (i + 1).ToString(); 
                    isChecked = isChecked 
                })
    }

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
