module ``TodoList tests``

open System
open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.TodoList


let shortTextLength = 3
let longTextLength  = 3
let shortText       = String ('a', shortTextLength)
let longText        = String ('a', longTextLength)

let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType

// Definition

[<Test>]
let ``TodoList fullname should be Samples.Domains.TodoList`` () =
    moduleType.FullName 
    |> should equal "Samples.Domains.TodoList"

[<Test>]
let ``TodoList Event should be defined as expected`` () =
    eventType 
    |> shouldBeAUnion
        { 
            unionName = "Event";
            cases = 
            [
                { caseName = "WasCreated";  types = [ typedefof<string>; ] }; 
                { caseName = "TitleWasUpdated";  types = [ typedefof<string>; ] }; 
                { caseName = "TaskWasAdded";  types = [ typedefof<int>; typedefof<string>; ] }; 
                { caseName = "TaskWasUpdated";  types = [ typedefof<int>; typedefof<string>; ] }; 
                { caseName = "TaskWasRemoved";  types = [ typedefof<int>; ] }; 
                { caseName = "TaskWasChecked";  types = [ typedefof<int>; ] }; 
                { caseName = "TaskWasUnchecked";  types = [ typedefof<int>; ] }; 
            ]
        }

[<Test>]
let ``TodoList Command should be defined as expected`` () =
    commandType 
    |> shouldBeAUnion
        { 
            unionName = "Command";
            cases = 
            [
                { caseName = "Create";  types = [ typedefof<string>; ] }; 
                { caseName = "UpdateTitle";  types = [ typedefof<string>; ] }; 
                { caseName = "AddTask";  types = [ typedefof<string>; ] }; 
                { caseName = "UpdateTask";  types = [ typedefof<int>; typedefof<string>; ] }; 
                { caseName = "RemoveTask";  types = [ typedefof<int>; ] }; 
                { caseName = "CheckTask";  types = [ typedefof<int>; ] }; 
                { caseName = "UncheckTask";  types = [ typedefof<int>; ] }; 
                { caseName = "RemoveAllTasks";  types = [ ] }; 
                { caseName = "RemoveAllCheckedTasks";  types = [ ] }; 
                { caseName = "CheckAllTasks";  types = [ ] }; 
                { caseName = "UncheckAllTasks";  types = [ ] }; 
            ]
        }


// Runtime

let initTitle = "TodoList initial title"
let title = "TodoList new title"
let initText = "Task new text"

let createdState()   = applyEvents2 init apply [ WasCreated initTitle ]
let oneTaskState()   = applyEvents2 (createdState()) apply [ TaskWasAdded (1, "task #1") ]
let twoTaskState()   = applyEvents2 (oneTaskState()) apply [ TaskWasAdded (2, "task #2") ]
let threeTaskState() = applyEvents2 (twoTaskState()) apply [ TaskWasAdded (3, "task #3") ]
let threeTaskTwoCheckedState() = applyEvents2 (threeTaskState()) apply [ TaskWasChecked 1; TaskWasChecked 3; ]
let threeTaskAllCheckedState() = applyEvents2 (threeTaskTwoCheckedState()) apply [ TaskWasChecked 2; ]


[<Test>]
let ``TodoList initial state should be null`` () =
    init 
    |> should be Null

[<Test>]
let ``TodoList acting with Create command with initTitle over initial state should give WasCreated with the same initTitle`` () =
    act (Create initTitle) init
    |> should equal [ WasCreated initTitle ]

[<Test>]
let ``TodoList acting with Create command over some state should fail`` () =
    (fun () -> act (Create initTitle) (createdState()) |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with UpdateTitle command over initial state should fail`` () =
    (fun () -> act (UpdateTitle initTitle) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with UpdateTitle command with initTitle over some state should give no events`` () =
    act (UpdateTitle initTitle) (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UpdateTitle command with title over some state should give TitleWasUpdated with the same title`` () =
    act (UpdateTitle title) (createdState())
    |> should equal [ TitleWasUpdated title ]

[<Test>]
let ``TodoList acting with AddTask command over initial state should fail`` () =
    (fun () -> act (AddTask initTitle) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with AddTask command with initText over some state should give TaskWasAdded with the same initText`` () =
    act (AddTask initText) (createdState())
    |> should equal [ TaskWasAdded (1, initText) ]

[<Test>]
let ``TodoList acting with UpdateTask command over initial state should fail`` () =
    (fun () -> act (UpdateTask (1, initTitle)) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with UpdateTask command with initTitle over some state should give no events`` () =
    act (UpdateTask (1, initTitle)) (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UpdateTask command with title over some state should give TaskWasUpdated with the same title`` () =
    act (UpdateTask (1, title)) (twoTaskState())
    |> should equal [ TaskWasUpdated (1, title) ]

[<Test>]
let ``TodoList acting with RemoveTask command over initial state should fail`` () =
    (fun () -> act (RemoveTask 1) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with RemoveTask command with initTitle over some state should give no events`` () =
    act (RemoveTask 1) (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with RemoveTask command with title over some state should give TaskWasRemoved with the same title`` () =
    act (RemoveTask 1) (twoTaskState())
    |> should equal [ TaskWasRemoved 1 ]

[<Test>]
let ``TodoList acting with CheckTask command over initial state should fail`` () =
    (fun () -> act (CheckTask 1) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with CheckTask command with id over empty state should give no events`` () =
    act (CheckTask 1) (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with CheckTask command with id over checked task should give no events`` () =
    act (CheckTask 1) (threeTaskTwoCheckedState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with CheckTask command with id over some state should give TaskWasChecked with the same id`` () =
    act (CheckTask 1) (threeTaskState())
    |> should equal [ TaskWasChecked 1 ]

[<Test>]
let ``TodoList acting with UncheckTask command over initial state should fail`` () =
    (fun () -> act (UncheckTask 1) init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with UncheckTask command with id over empty state should give no events`` () =
    act (UncheckTask 1) (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UncheckTask command with id over unchecked task should give no events`` () =
    act (UncheckTask 1) (threeTaskState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UncheckTask command with id over checked task should give TaskWasUnchecked with the same id`` () =
    act (UncheckTask 1) (threeTaskTwoCheckedState())
    |> should equal [ TaskWasUnchecked 1 ]

[<Test>]
let ``TodoList acting with RemoveAllTasks command over initial state should fail`` () =
    (fun () -> act RemoveAllTasks init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with RemoveAllTasks command over empty state should give no events`` () =
    act RemoveAllTasks (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with RemoveAllTasks command over some state should give various TaskWasRemoved events`` () =
    act RemoveAllTasks (threeTaskTwoCheckedState())
    |> should equal [ TaskWasRemoved 1; TaskWasRemoved 2; TaskWasRemoved 3; ]

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over initial state should fail`` () =
    (fun () -> act RemoveAllCheckedTasks init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over empty state should give no events`` () =
    act RemoveAllCheckedTasks (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over state with no checked tasks should give no events`` () =
    act RemoveAllCheckedTasks (threeTaskState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over some state should give various TaskWasRemoved events`` () =
    act RemoveAllCheckedTasks (threeTaskTwoCheckedState())
    |> should equal [ TaskWasRemoved 1; TaskWasRemoved 3; ]

[<Test>]
let ``TodoList acting with CheckAllTasks command over initial state should fail`` () =
    (fun () -> act CheckAllTasks init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with CheckAllTasks command over empty state should give no events`` () =
    act CheckAllTasks (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with CheckAllTasks command over state with no unchecked tasks should give no events`` () =
    act CheckAllTasks (threeTaskAllCheckedState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with CheckAllTasks command over some state should give various TaskWasChecked events`` () =
    act CheckAllTasks (threeTaskTwoCheckedState())
    |> should equal [ TaskWasChecked 2; ]

[<Test>]
let ``TodoList acting with UncheckAllTasks command over initial state should fail`` () =
    (fun () -> act UncheckAllTasks init |> ignore)
    |> should throw typeof<MatchFailureException>

[<Test>]
let ``TodoList acting with UncheckAllTasks command over empty state should give no events`` () =
    act UncheckAllTasks (createdState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UncheckAllTasks command over state with no unchecked tasks should give no events`` () =
    act UncheckAllTasks (threeTaskState())
    |> should equal [ ]

[<Test>]
let ``TodoList acting with UncheckAllTasks command over some state should give various TaskWasUnchecked events`` () =
    act UncheckAllTasks (threeTaskTwoCheckedState())
    |> should equal [ TaskWasUnchecked 1; TaskWasUnchecked 3; ]
