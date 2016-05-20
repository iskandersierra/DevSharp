namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList tests``() = 
    
    // initialState

    [<Test>] member test.
                ``initial state should be None`` () =
                Assert.That (initialState, Is.EqualTo None )

    // processCommand

    [<Test>] member test.
                ``process Create from None should give Created`` () =
                let state = initialState
                let command = Create "Title of todo list"
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Created "Title of todo list" ] )

    [<Test>] member test.
                ``process non-Create from None should not be supported`` () =
                let state = None
                let command = AddTask "Task description"
                let call = fun () -> processCommand state command |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] member test.
                ``process Create from some State should not be supported`` () =
                let state = Some { title = "a title"; nextTaskId = 1; tasks = [] }
                let command = Create "Title of todo list"
                let call = fun () -> processCommand state command |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] member test.
                ``process UpdateTitle from some State should give TitleUpdated`` () =
                let state = Some { title = "a title"; nextTaskId = 1; tasks = [] }
                let command = UpdateTitle "New title for todo list"
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TitleUpdated "New title for todo list" ] )

    [<Test>] member test.
                ``process UpdateTitle with same title  from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 1; tasks = [] }
                let command = UpdateTitle "a title"
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.empty )

    [<Test>] member test.
                ``process AddTask from some State should give TaskAdded`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [] }
                let command = AddTask "New task"
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskAdded (3, "New task") ] )

    [<Test>] member test.
                ``process UpdateTask from some State should give TaskUpdated`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = UpdateTask (1, "New task")
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskUpdated (1, "New task") ] )

    [<Test>] member test.
                ``process UpdateTask of non-existing task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = UpdateTask (3, "New task")
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process RemoveTask from some State should give TaskRemoved`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = RemoveTask 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ TaskRemoved 1 ] )

    [<Test>] member test.
                ``process RemoveTask of non-existing task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = RemoveTask 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Check from some State should give Checked`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Check 2
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Checked 2 ] )

    [<Test>] member test.
                ``process Check of already checked task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Check 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Check of non-existing task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Check 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Uncheck from some State should give Unchecked`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Uncheck 1
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo [ Unchecked 1 ] )

    [<Test>] member test.
                ``process Uncheck of already unchecked task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Uncheck 2
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )

    [<Test>] member test.
                ``process Uncheck of non-existing task from some State should give nothing`` () =
                let state = Some { title = "a title"; nextTaskId = 3; tasks = [ { id = 1; text = "task #1"; isChecked = true }; { id = 2; text = "task #2"; isChecked = false } ] }
                let command = Uncheck 3
                let events = processCommand state command
                in
                Assert.That (events, Is.EquivalentTo List.Empty )
