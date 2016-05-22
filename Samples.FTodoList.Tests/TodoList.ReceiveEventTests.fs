namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList receiveEvent tests``() = 

    [<Test>] 
    member test.``receive non-Created from None should not be supported`` () =
        testReceiveEventIsInvalid
            receiveEvent
            initialState
            ( TaskAdded (3, "Task description") )

    [<Test>] 
    member test.``receive Created from some State should not be supported`` () =
        testReceiveEventIsInvalid
            receiveEvent
            emptyState
            ( Created defaultTitle )

    [<Test>] 
    member test.``receive Created from None should give some State`` () =
        testReceiveEventIsValid
            receiveEvent
            initialState
            ( Created defaultTitle )
            emptyState

    [<Test>] 
    member test.``receive TitleUpdated from some State should give some State`` () =
        testReceiveEventIsValid
            receiveEvent
            emptyState
            ( TitleUpdated "New title" )
            ( emptyStateTitle "New title" )

    [<Test>] 
    member test.``receive TaskAdded from some State should give some State`` () =
        testReceiveEventIsValid
            receiveEvent
            ( Some (createState [true; false]) )
            ( TaskAdded (3, "task #3") )
            ( Some (createState [true; false; false]) )

    [<Test>] 
    member test.``receive TaskUpdated from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            receiveEvent
            ( Some state )
            ( TaskUpdated (1, "new task #1") )
            ( Some { state with tasks = state.tasks |> List.map (fun t -> if t.id = 1 then {t with text = "new task #1"} else t) } )

    [<Test>] 
    member test.``receive TaskRemoved from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            receiveEvent
            ( Some state )
            ( TaskRemoved 1 )
            ( Some { state with tasks = state.tasks |> List.filter (fun t -> t.id <> 1) } )

    [<Test>] 
    member test.``receive Checked from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            receiveEvent
            ( Some state )
            ( Checked 2 )
            ( Some (createState [true; true]) )

    [<Test>] 
    member test.``receive Unchecked from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            receiveEvent
            ( Some state )
            ( Unchecked 1 )
            ( Some (createState [false; false]) )
