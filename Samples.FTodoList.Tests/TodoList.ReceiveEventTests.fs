namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList apply tests``() = 

    [<Test>] 
    member test.``receive non-WasCreated from None should not be supported`` () =
        testReceiveEventIsInvalid
            apply
            initialState
            ( TaskWasAdded (3, "Task description") )

    [<Test>] 
    member test.``receive WasCreated from some State should not be supported`` () =
        testReceiveEventIsInvalid
            apply
            emptyState
            ( WasCreated defaultTitle )

    [<Test>] 
    member test.``receive WasCreated from None should give some State`` () =
        testReceiveEventIsValid
            apply
            initialState
            ( WasCreated defaultTitle )
            emptyState

    [<Test>] 
    member test.``receive TitleWasUpdated from some State should give some State`` () =
        testReceiveEventIsValid
            apply
            emptyState
            ( TitleWasUpdated "New title" )
            ( emptyStateTitle "New title" )

    [<Test>] 
    member test.``receive TaskWasAdded from some State should give some State`` () =
        testReceiveEventIsValid
            apply
            ( Some (createState [true; false]) )
            ( TaskWasAdded (3, "task #3") )
            ( Some (createState [true; false; false]) )

    [<Test>] 
    member test.``receive TaskWasUpdated from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            apply
            ( Some state )
            ( TaskWasUpdated (1, "new task #1") )
            ( Some { state with tasks = state.tasks |> List.map (fun t -> if t.id = 1 then {t with text = "new task #1"} else t) } )

    [<Test>] 
    member test.``receive TaskWasRemoved from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            apply
            ( Some state )
            ( TaskWasRemoved 1 )
            ( Some { state with tasks = state.tasks |> List.filter (fun t -> t.id <> 1) } )

    [<Test>] 
    member test.``receive TaskWasChecked from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            apply
            ( Some state )
            ( TaskWasChecked 2 )
            ( Some (createState [true; true]) )

    [<Test>] 
    member test.``receive TaskWasUnchecked from some State should give some State`` () =
        let state = createState [true; false]
        testReceiveEventIsValid
            apply
            ( Some state )
            ( TaskWasUnchecked 1 )
            ( Some (createState [false; false]) )
