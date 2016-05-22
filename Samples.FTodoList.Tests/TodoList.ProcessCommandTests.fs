namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList processCommand tests``() = 

    [<Test>] 
    member test.``process Create from some State should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            emptyState
            ( Create defaultTitle )

    [<Test>] 
    member test.``process UpdateTitle from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( UpdateTitle "Some new title" )

    [<Test>] 
    member test.``process AddTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( AddTask "Task description" )

    [<Test>] 
    member test.``process UpdateTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( UpdateTask (1, "Task description") )

    [<Test>] 
    member test.``process RemoveTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( RemoveTask 1 )

    [<Test>] 
    member test.``process Check from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( Check 1 )

    [<Test>] 
    member test.``process RemoveAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            RemoveAll

    [<Test>] 
    member test.``process RemoveAllChecked from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            RemoveAllChecked

    [<Test>] 
    member test.``process CheckAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            CheckAll

    [<Test>] 
    member test.``process UncheckAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            UncheckAll

    [<Test>] 
    member test.``process Uncheck from None should not be supported`` () =
        testProcessCommandIsInvalid 
            processCommand
            initialState
            ( Uncheck 1 )

    [<Test>] 
    member test.``process Create from None should give Created`` () =
        testProcessCommandIsValid 
            processCommand
            initialState
            ( Create defaultTitle )
            [ Created defaultTitle ]

    [<Test>] 
    member test.``process UpdateTitle from some State should give TitleUpdated`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            ( UpdateTitle "New title for todo list" )
            [ TitleUpdated "New title for todo list" ]

    [<Test>] 
    member test.``process UpdateTitle with same title  from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            ( UpdateTitle defaultTitle )
            List.empty

    [<Test>] 
    member test.``process AddTask from some State should give TaskAdded`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            ( AddTask "New task" )
            [ TaskAdded (1, "New task") ]

    [<Test>] 
    member test.``process UpdateTask from some State should give TaskUpdated`` () =
        testProcessCommandIsValid 
            processCommand
            ( Some (createState [true; false]) )
            ( UpdateTask (1, "New task") )
            [ TaskUpdated (1, "New task") ]

    [<Test>] 
    member test.``process UpdateTask of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( UpdateTask (3, "New task") )
            List.empty

    [<Test>] 
    member test.``process UpdateTask with same text from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( UpdateTask (2, "task #2") )
            List.empty

    [<Test>] 
    member test.``process RemoveTask from some State should give TaskRemoved`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( RemoveTask 1 )
            [ TaskRemoved 1 ]

    [<Test>] 
    member test.``process RemoveTask of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( RemoveTask 3 )
            List.empty

    [<Test>] 
    member test.``process Check from some State should give Checked`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Check 2 )
            [ Checked 2 ]

    [<Test>] 
    member test.``process Check of already checked task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Check 1 )
            List.empty

    [<Test>] 
    member test.``process Check of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Check 3 )
            List.empty

    [<Test>] 
    member test.``process Uncheck from some State should give Unchecked`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Uncheck 1 )
            [ Unchecked 1 ]

    [<Test>] 
    member test.``process Uncheck of already unchecked task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Uncheck 2 )
            List.empty

    [<Test>] 
    member test.``process Uncheck of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            ( Uncheck 3 )
            List.empty

    [<Test>] 
    member test.``process RemoveAll from some State should give many TaskRemoved`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false]) )
            RemoveAll
            [ TaskRemoved 1; TaskRemoved 2 ]

    [<Test>] 
    member test.``process RemoveAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            RemoveAll
            List.empty

    [<Test>] 
    member test.``process RemoveAllChecked from some State should give many TaskRemoved`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false; true]) )
            RemoveAllChecked
            [ TaskRemoved 1; TaskRemoved 3 ]

    [<Test>] 
    member test.``process RemoveAllChecked from some State with no checked tasks should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [false; false]) )
            RemoveAllChecked
            List.empty

    [<Test>] 
    member test.``process RemoveAllChecked from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            RemoveAllChecked
            List.empty

    [<Test>] 
    member test.``process CheckAll from some State should give many Checked`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [false; true; false]) )
            CheckAll
            [ Checked 1; Checked 3 ]

    [<Test>] 
    member test.``process CheckAll from some State with no checked tasks should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; true]) )
            CheckAll
            List.empty

    [<Test>] 
    member test.``process CheckAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            CheckAll
            List.empty

    [<Test>] 
    member test.``process UncheckAll from some State should give many Unchecked`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [true; false; true]) )
            UncheckAll
            [ Unchecked 1; Unchecked 3 ]

    [<Test>] 
    member test.``process UncheckAll from some State with no unchecked tasks should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            ( Some (createState [false; false]) )
            UncheckAll
            List.empty

    [<Test>] 
    member test.``process UncheckAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            processCommand
            emptyState
            UncheckAll
            List.empty
               