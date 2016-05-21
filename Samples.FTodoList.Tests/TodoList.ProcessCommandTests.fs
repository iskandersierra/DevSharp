namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList processCommand tests``() = 

    [<Test>] member test.
                ``process non-Create from None should not be supported`` () =
                let state = None
                let command = AddTask "Task description"
                let call = fun () -> processCommand state command |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] member test.
                ``process Create from some State should not be supported`` () =
                let state = emptyState
                let command = Create defaultTitle
                let call = fun () -> processCommand state command |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] 
    member test.``process Create from None should give Created`` () =
        testProcessCommandIsValid processCommand initialState (Create defaultTitle) [ Created defaultTitle ]

    [<Test>] 
    member test.``process UpdateTitle from some State should give TitleUpdated`` () =
        testProcessCommandIsValid processCommand emptyState (UpdateTitle "New title for todo list") [ TitleUpdated "New title for todo list" ]

    [<Test>] 
    member test.``process UpdateTitle with same title  from some State should give nothing`` () =
        testProcessCommandIsValid processCommand emptyState (UpdateTitle defaultTitle) [ ]

    [<Test>] 
    member test.``process AddTask from some State should give TaskAdded`` () =
        testProcessCommandIsValid processCommand emptyState (AddTask "New task") [ TaskAdded (1, "New task") ]

    [<Test>] 
    member test.``process UpdateTask from some State should give TaskUpdated`` () =
        testProcessCommandIsValid processCommand (Some (createState [true; false])) (UpdateTask (1, "New task")) [ TaskUpdated (1, "New task") ]

    [<Test>] member test.
                ``process UpdateTask of non-existing task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = UpdateTask (3, "New task")
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process UpdateTask with same text from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = UpdateTask (2, "task #2")
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process RemoveTask from some State should give TaskRemoved`` () =
                let state = Some (createState [true; false])
                let command = RemoveTask 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskRemoved 1 ] )

    [<Test>] member test.
                ``process RemoveTask of non-existing task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = RemoveTask 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Check from some State should give Checked`` () =
                let state = Some (createState [true; false])
                let command = Check 2
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Checked 2 ] )

    [<Test>] member test.
                ``process Check of already checked task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = Check 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Check of non-existing task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = Check 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Uncheck from some State should give Unchecked`` () =
                let state = Some (createState [true; false])
                let command = Uncheck 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Unchecked 1 ] )

    [<Test>] member test.
                ``process Uncheck of already unchecked task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = Uncheck 2
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Uncheck of non-existing task from some State should give nothing`` () =
                let state = Some (createState [true; false])
                let command = Uncheck 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process RemoveAll from some State should give many TaskRemoved`` () =
                let state = Some (createState [true; false])
                let command = RemoveAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskRemoved 1; TaskRemoved 2 ] )

    [<Test>] member test.
                ``process RemoveAll from some Empty list should give nothing`` () =
                let state = emptyState
                let command = RemoveAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process RemoveAllChecked from some State should give many TaskRemoved`` () =
                let state = Some (createState [true; false; true])
                let command = RemoveAllChecked
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskRemoved 1; TaskRemoved 3 ] )

    [<Test>] member test.
                ``process RemoveAllChecked from some State with no checked tasks should give nothing`` () =
                let state = Some (createState [false; false])
                let command = RemoveAllChecked
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process RemoveAllChecked from some Empty list should give nothing`` () =
                let state = emptyState
                let command = RemoveAllChecked
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process CheckAll from some State should give many Checked`` () =
                let state = Some (createState [false; true; false])
                let command = CheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Checked 1; Checked 3 ] )

    [<Test>] member test.
                ``process CheckAll from some State with no checked tasks should give nothing`` () =
                let state = Some (createState [true; true])
                let command = CheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process CheckAll from some Empty list should give nothing`` () =
                let state = emptyState
                let command = CheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process UncheckAll from some State should give many Unchecked`` () =
                let state = Some (createState [true; false; true])
                let command = UncheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Unchecked 1; Unchecked 3 ] )

    [<Test>] member test.
                ``process UncheckAll from some State with no unchecked tasks should give nothing`` () =
                let state = Some (createState [false; false])
                let command = UncheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process UncheckAll from some Empty list should give nothing`` () =
                let state = emptyState
                let command = UncheckAll
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )
               