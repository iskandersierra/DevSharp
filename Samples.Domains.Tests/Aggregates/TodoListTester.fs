module ``TodoList tests``

open System
open NUnit.Framework
open FsUnit
open DevSharp.Testing
open DevSharp.Testing.DomainTesting
open Samples.Domains.TodoList
open NUnit.Framework.Constraints


let shortTextLength = 3
let longTextLength  = 3
let shortText       = String ('a', shortTextLength)
let longText        = String ('a', longTextLength)

let eventType   = typedefof<Event>
let commandType = typedefof<Command>
let moduleType  = commandType.DeclaringType


[<SetUp>]
let testSetup () =
    TestContext.AddFormatter(ValueFormatterFactory(fun _ -> ValueFormatter(sprintf "%A")))

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
                { caseName = "WasCreated";  types = [ typedefof<TodoListTitle>; ] }; 
                { caseName = "TitleWasUpdated";  types = [ typedefof<TodoListTitle>; ] }; 
                { caseName = "TaskWasAdded";  types = [ typedefof<TaskId>; typedefof<TaskText>; ] }; 
                { caseName = "TaskWasUpdated";  types = [ typedefof<TaskId>; typedefof<TaskText>; ] }; 
                { caseName = "TaskWasRemoved";  types = [ typedefof<TaskId>; ] }; 
                { caseName = "TaskWasChecked";  types = [ typedefof<TaskId>; ] }; 
                { caseName = "TaskWasUnchecked";  types = [ typedefof<TaskId>; ] }; 
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
                { caseName = "Create";  types = [ typedefof<TodoListTitle>; ] }; 
                { caseName = "UpdateTitle";  types = [ typedefof<TodoListTitle>; ] }; 
                { caseName = "AddTask";  types = [ typedefof<TaskText>; ] }; 
                { caseName = "UpdateTask";  types = [ typedefof<TaskId>; typedefof<TaskText>; ] }; 
                { caseName = "RemoveTask";  types = [ typedefof<TaskId>; ] }; 
                { caseName = "CheckTask";  types = [ typedefof<TaskId>; ] }; 
                { caseName = "UncheckTask";  types = [ typedefof<TaskId>; ] }; 
                { caseName = "RemoveAllTasks";  types = [ ] }; 
                { caseName = "RemoveAllCheckedTasks";  types = [ ] }; 
                { caseName = "CheckAllTasks";  types = [ ] }; 
                { caseName = "UncheckAllTasks";  types = [ ] }; 
            ]
        }


// Runtime

let initTitle = TodoListTitle "TodoList initial title"
let title = TodoListTitle "TodoList new title"
let initText = TaskText "Task new text"
let newText = TaskText "Task other text"
let taskText str = TaskText str

let createdState()   = applyEvents2 init apply [ WasCreated initTitle ]
let oneTaskState()   = applyEvents2 (createdState()) apply [ TaskWasAdded (TaskId 1, TaskText "task #1") ]
let twoTaskState()   = applyEvents2 (oneTaskState()) apply [ TaskWasAdded (TaskId 2, TaskText "task #2") ]
let threeTaskState() = applyEvents2 (twoTaskState()) apply [ TaskWasAdded (TaskId 3, TaskText "task #3") ]
let threeTaskTwoCheckedState() = applyEvents2 (threeTaskState()) apply [ TaskWasChecked <| TaskId 1; TaskWasChecked <| TaskId 3; ]
let threeTaskAllCheckedState() = applyEvents2 (threeTaskTwoCheckedState()) apply [ TaskWasChecked <| TaskId 2; ]


[<Test>]
let ``TodoList initial state should be null`` () =
    init 
    |> should be Null

[<Test>]
let ``TodoList acting with Create command with initTitle over initial state should give WasCreated with the same initTitle`` () =
    act (Create initTitle) init
    |> should equal (Some [ WasCreated initTitle ])

[<Test>]
let ``TodoList acting with Create command over some state should return None`` () =
    act (Create initTitle) (createdState())
    |> should be Null

[<Test>]
let ``TodoList acting with UpdateTitle command over initial state should return None`` () =
    act (UpdateTitle initTitle) init
    |> should be Null

[<Test>]
let ``TodoList acting with UpdateTitle command with initTitle over some state should give no events`` () =
    act (UpdateTitle initTitle) (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UpdateTitle command with title over some state should give TitleWasUpdated with the same title`` () =
    act (UpdateTitle title) (createdState())
    |> should equal (Some [ TitleWasUpdated title ])

[<Test>]
let ``TodoList acting with AddTask command over initial state should return None`` () =
    act (AddTask initText) init 
    |> should be Null

[<Test>]
let ``TodoList acting with AddTask command with initText over some state should give TaskWasAdded with the same initText`` () =
    act (AddTask initText) (createdState())
    |> should equal (Some [ TaskWasAdded (TaskId 1, initText) ])

[<Test>]
let ``TodoList acting with UpdateTask command over initial state should return None`` () =
    act (UpdateTask (TaskId 1, initText)) init 
    |> should be Null

[<Test>]
let ``TodoList acting with UpdateTask command with initTitle over some state should give no events`` () =
    act (UpdateTask (TaskId 1, initText)) (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UpdateTask command with title over some state should give TaskWasUpdated with the same title`` () =
    act (UpdateTask (TaskId 1, initText)) (twoTaskState())
    |> should equal (Some [ TaskWasUpdated (TaskId 1, initText) ])

[<Test>]
let ``TodoList acting with RemoveTask command over initial state should return None`` () =
    act (RemoveTask <| TaskId 1) init
    |> should be Null

[<Test>]
let ``TodoList acting with RemoveTask command with initTitle over some state should give no events`` () =
    act (RemoveTask <| TaskId 1) (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with RemoveTask command with title over some state should give TaskWasRemoved with the same title`` () =
    act (RemoveTask <| TaskId 1) (twoTaskState())
    |> should equal (Some [ TaskWasRemoved <| TaskId 1 ])

[<Test>]
let ``TodoList acting with CheckTask command over initial state should return None`` () =
    act (CheckTask <| TaskId 1) init 
    |> should be Null

[<Test>]
let ``TodoList acting with CheckTask command with id over empty state should give no events`` () =
    act (CheckTask <| TaskId 1) (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with CheckTask command with id over checked task should give no events`` () =
    act (CheckTask <| TaskId 1) (threeTaskTwoCheckedState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with CheckTask command with id over some state should give TaskWasChecked with the same id`` () =
    act (CheckTask <| TaskId 1) (threeTaskState())
    |> should equal (Some [ TaskWasChecked <| TaskId 1 ])

[<Test>]
let ``TodoList acting with UncheckTask command over initial state should return None`` () =
    act (UncheckTask <| TaskId 1) init 
    |> should be Null

[<Test>]
let ``TodoList acting with UncheckTask command with id over empty state should give no events`` () =
    act (UncheckTask <| TaskId 1) (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UncheckTask command with id over unchecked task should give no events`` () =
    act (UncheckTask <| TaskId 1) (threeTaskState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UncheckTask command with id over checked task should give TaskWasUnchecked with the same id`` () =
    act (UncheckTask <| TaskId 1) (threeTaskTwoCheckedState())
    |> should equal (Some [ TaskWasUnchecked <| TaskId 1 ])

[<Test>]
let ``TodoList acting with RemoveAllTasks command over initial state should return None`` () =
    act RemoveAllTasks init 
    |> should be Null

[<Test>]
let ``TodoList acting with RemoveAllTasks command over empty state should give no events`` () =
    act RemoveAllTasks (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with RemoveAllTasks command over some state should give various TaskWasRemoved events`` () =
    act RemoveAllTasks (threeTaskTwoCheckedState())
    |> should equal (Some [ TaskWasRemoved <| TaskId 1; TaskWasRemoved <| TaskId 2; TaskWasRemoved <| TaskId 3; ])

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over initial state should return None`` () =
    act RemoveAllCheckedTasks init 
    |> should be Null

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over empty state should give no events`` () =
    act RemoveAllCheckedTasks (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over state with no checked tasks should give no events`` () =
    act RemoveAllCheckedTasks (threeTaskState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with RemoveAllCheckedTasks command over some state should give various TaskWasRemoved events`` () =
    act RemoveAllCheckedTasks (threeTaskTwoCheckedState())
    |> should equal (Some [ TaskWasRemoved <| TaskId 1; TaskWasRemoved <| TaskId 3; ])

[<Test>]
let ``TodoList acting with CheckAllTasks command over initial state should return None`` () =
    act CheckAllTasks init 
    |> should be Null

[<Test>]
let ``TodoList acting with CheckAllTasks command over empty state should give no events`` () =
    act CheckAllTasks (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with CheckAllTasks command over state with no unchecked tasks should give no events`` () =
    act CheckAllTasks (threeTaskAllCheckedState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with CheckAllTasks command over some state should give various TaskWasChecked events`` () =
    act CheckAllTasks (threeTaskTwoCheckedState())
    |> should equal (Some [ TaskWasChecked <| TaskId 2; ])

[<Test>]
let ``TodoList acting with UncheckAllTasks command over initial state should return None`` () =
    act UncheckAllTasks init 
    |> should be Null

[<Test>]
let ``TodoList acting with UncheckAllTasks command over empty state should give no events`` () =
    act UncheckAllTasks (createdState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UncheckAllTasks command over state with no unchecked tasks should give no events`` () =
    act UncheckAllTasks (threeTaskState())
    |> should equal (Some List.empty<Event>)

[<Test>]
let ``TodoList acting with UncheckAllTasks command over some state should give various TaskWasUnchecked events`` () =
    act UncheckAllTasks (threeTaskTwoCheckedState())
    |> should equal (Some [ TaskWasUnchecked <| TaskId 1; TaskWasUnchecked <| TaskId 3; ])
