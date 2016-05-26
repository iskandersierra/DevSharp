namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList act tests``() = 

    [<Test>] 
    member test.``process Create from some State should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            emptyState
            ( Create defaultTitle )

    [<Test>] 
    member test.``process UpdateTitle from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( UpdateTitle "Some new title" )

    [<Test>] 
    member test.``process AddTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( AddTask "Task description" )

    [<Test>] 
    member test.``process UpdateTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( UpdateTask (1, "Task description") )

    [<Test>] 
    member test.``process RemoveTask from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( RemoveTask 1 )

    [<Test>] 
    member test.``process Check from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( CheckTask 1 )

    [<Test>] 
    member test.``process RemoveAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            RemoveAllTasks

    [<Test>] 
    member test.``process RemoveAllChecked from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            RemoveAllCheckedTasks

    [<Test>] 
    member test.``process CheckAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            CheckAllTasks

    [<Test>] 
    member test.``process UncheckAll from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            UncheckAllTasks

    [<Test>] 
    member test.``process Uncheck from None should not be supported`` () =
        testProcessCommandIsInvalid 
            act
            init
            ( UncheckTask 1 )

    [<Test>] 
    member test.``process Create from None should give Created`` () =
        testProcessCommandIsValid 
            act
            init
            ( Create defaultTitle )
            [ WasCreated defaultTitle ]

    [<Test>] 
    member test.``process UpdateTitle from some State should give TitleUpdated`` () =
        testProcessCommandIsValid
            act
            emptyState
            ( UpdateTitle "New title for todo list" )
            [ TitleWasUpdated "New title for todo list" ]

    [<Test>] 
    member test.``process UpdateTitle with same title  from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            emptyState
            ( UpdateTitle defaultTitle )
            List.empty

    [<Test>] 
    member test.``process AddTask from some State should give TaskAdded`` () =
        testProcessCommandIsValid
            act
            emptyState
            ( AddTask "New task" )
            [ TaskWasAdded (1, "New task") ]

    [<Test>] 
    member test.``process UpdateTask from some State should give TaskUpdated`` () =
        testProcessCommandIsValid 
            act
            ( Some (createState [true; false]) )
            ( UpdateTask (1, "New task") )
            [ TaskWasUpdated (1, "New task") ]

    [<Test>] 
    member test.``process UpdateTask of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( UpdateTask (3, "New task") )
            List.empty

    [<Test>] 
    member test.``process UpdateTask with same text from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( UpdateTask (2, "task #2") )
            List.empty

    [<Test>] 
    member test.``process RemoveTask from some State should give TaskRemoved`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( RemoveTask 1 )
            [ TaskWasRemoved 1 ]

    [<Test>] 
    member test.``process RemoveTask of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( RemoveTask 3 )
            List.empty

    [<Test>] 
    member test.``process Check from some State should give Checked`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( CheckTask 2 )
            [ TaskWasChecked 2 ]

    [<Test>] 
    member test.``process Check of already checked task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( CheckTask 1 )
            List.empty

    [<Test>] 
    member test.``process Check of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( CheckTask 3 )
            List.empty

    [<Test>] 
    member test.``process Uncheck from some State should give Unchecked`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( UncheckTask 1 )
            [ TaskWasUnchecked 1 ]

    [<Test>] 
    member test.``process Uncheck of already unchecked task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( UncheckTask 2 )
            List.empty

    [<Test>] 
    member test.``process Uncheck of non-existing task from some State should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            ( UncheckTask 3 )
            List.empty

    [<Test>] 
    member test.``process RemoveAll from some State should give many TaskRemoved`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false]) )
            RemoveAllTasks
            [ TaskWasRemoved 1; TaskWasRemoved 2 ]

    [<Test>] 
    member test.``process RemoveAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            act
            emptyState
            RemoveAllTasks
            List.empty

    [<Test>] 
    member test.``process RemoveAllChecked from some State should give many TaskRemoved`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false; true]) )
            RemoveAllCheckedTasks
            [ TaskWasRemoved 1; TaskWasRemoved 3 ]

    [<Test>] 
    member test.``process RemoveAllChecked from some State with no checked tasks should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [false; false]) )
            RemoveAllCheckedTasks
            List.empty

    [<Test>] 
    member test.``process RemoveAllChecked from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            act
            emptyState
            RemoveAllCheckedTasks
            List.empty

    [<Test>] 
    member test.``process CheckAll from some State should give many Checked`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [false; true; false]) )
            CheckAllTasks
            [ TaskWasChecked 1; TaskWasChecked 3 ]

    [<Test>] 
    member test.``process CheckAll from some State with no checked tasks should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; true]) )
            CheckAllTasks
            List.empty

    [<Test>] 
    member test.``process CheckAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            act
            emptyState
            CheckAllTasks
            List.empty

    [<Test>] 
    member test.``process UncheckAll from some State should give many Unchecked`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [true; false; true]) )
            UncheckAllTasks
            [ TaskWasUnchecked 1; TaskWasUnchecked 3 ]

    [<Test>] 
    member test.``process UncheckAll from some State with no unchecked tasks should give nothing`` () =
        testProcessCommandIsValid
            act
            ( Some (createState [false; false]) )
            UncheckAllTasks
            List.empty

    [<Test>] 
    member test.``process UncheckAll from some Empty list should give nothing`` () =
        testProcessCommandIsValid
            act
            emptyState
            UncheckAllTasks
            List.empty
               