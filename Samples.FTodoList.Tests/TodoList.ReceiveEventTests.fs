namespace Samples.FTodoList.Tests

open System
open NUnit.Framework
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList receiveEvent tests``() = 

    [<Test>] member test.
                ``receive non-Created from None should not be supported`` () =
                let state = None
                let event = TaskAdded (3, "Task description")
                let call = fun () -> receiveEvent state event |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] member test.
                ``receive Created from some State should not be supported`` () =
                let state = emptyState
                let event = Created defaultTitle
                let call = fun () -> receiveEvent state event |> ignore
                in
                Assert.That (call, Throws.TypeOf<NotSupportedException>())

    [<Test>] member test.
                ``receive Created from None should give some State`` () =
                let state = initialState
                let event = Created defaultTitle
                let state2 = receiveEvent state event
                in
                Assert.That (state2, Is.EqualTo emptyState )

    [<Test>] member test.
                ``receive TitleUpdated from some State should give some State`` () =
                let state = emptyState
                let event = TitleUpdated "New title"
                let state2 = receiveEvent state event
                in
                Assert.That (state2, Is.EqualTo (emptyStateTitle "New title") )

    [<Test>] member test.
                ``receive TaskAdded from some State should give some State`` () =
                let state = Some (createState [true; false])
                let event = TaskAdded (3, "task #3")
                let state2 = receiveEvent state event
                in
                Assert.That (state2, Is.EqualTo (Some (createState [true; false; false])) )

    [<Test>] member test.
                ``receive TaskUpdated from some State should give some State`` () =
                let state1 = createState [true; false]
                let state = Some state1
                let event = TaskUpdated (1, "new task #1")
                let state2 = receiveEvent state event
                let expectedState = { state1 with tasks = state1.tasks |> List.map (fun t -> if t.id = 1 then {t with text = "new task #1"} else t) }
                in
                Assert.That (state2, Is.EqualTo (Some expectedState) )

    [<Test>] member test.
                ``receive TaskRemoved from some State should give some State`` () =
                let state1 = createState [true; false]
                let state = Some state1
                let event = TaskRemoved 1
                let state2 = receiveEvent state event
                let expectedState = { state1 with tasks = state1.tasks |> List.filter (fun t -> t.id <> 1) }
                in
                Assert.That (state2, Is.EqualTo (Some expectedState) )

    [<Test>] member test.
                ``receive Checked from some State should give some State`` () =
                let state = Some (createState [true; false])
                let event = Checked 2
                let state2 = receiveEvent state event
                in
                Assert.That (state2, Is.EqualTo (Some (createState [true; true])) )

    [<Test>] member test.
                ``receive Unchecked from some State should give some State`` () =
                let state = Some (createState [true; false])
                let event = Unchecked 1
                let state2 = receiveEvent state event
                in
                Assert.That (state2, Is.EqualTo (Some (createState [false; false])) )
