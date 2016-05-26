module Samples.FTodoList.TodoList

open System
open DevFSharp.Validations

type TodoListTitle = string
type TaskId        = int
type TaskText      = string

type Event = 
    | Created      of TodoListTitle     // [ 'text' ]
    | TitleUpdated of TodoListTitle     // [ 'text' ]
    | TaskAdded    of TaskId * TaskText      // [ 1, 'text' ]
    | TaskUpdated  of TaskId * TaskText      // [ 1, 'text' ]
    | TaskRemoved  of TaskId               // [ 1 ]
    | Checked      of TaskId               // [ 1 ]
    | Unchecked    of TaskId               // [ 1 ]
    
 // POST: http://example.com/api/Samples.TodoList/12345/Create [ 'Ejemplo' ]
type Command =
    | Create      of TodoListTitle      // [ 'Ejemplo' ]
    | UpdateTitle of TodoListTitle      // [ 'Ejemplo2' ]
    | AddTask     of TaskText             // [ 'Things to do later' ] 
    | UpdateTask  of TaskId * TaskText       // [ 1, 'More things to do later' ]
    | RemoveTask  of TaskId                // [ 1 ]
    | Check       of TaskId                // [ 1 ]
    | Uncheck     of TaskId                // [ 1 ]
    | RemoveAll                         // []
    | RemoveAllChecked                  // []
    | CheckAll                          // []    
    | UncheckAll                        // []

type State =                            // { title: 'Ejemplo', nextTaskId: 3, tasks: [ {id: 1, text: 'abc', isChecked: true}, {id: 2, text: 'def', isChecked: false} ] }
    { 
        title:      TodoListTitle; 
        nextTaskId: TaskId; 
        tasks:      TodoTask list;
    }    
and  TodoTask =
    { 
        id:        TaskId; 
        text:      TaskText; 
        isChecked: bool;
    }

//let act astate command =
//    match command with
//        | Create title

let processCommand astate command = 
    match astate with
        | None ->
            match command with
                | Create title ->
                    [ Created title 
                    ]
//                // This will throw a MatchException anyway
//                | _ -> 
//                    failProcessCommand astate command
                
        | Some state ->
            match command with
                | UpdateTitle newTitle -> 
                    if state.title <> newTitle 
                    then [ TitleUpdated newTitle ] 
                    else []
                | AddTask text ->
                    [ TaskAdded (state.nextTaskId, text) 
                    ]
                | UpdateTask (id, text) ->
                    state.tasks 
                    |> List.filter (fun t -> t.id = id && t.text <> text) 
                    |> List.map (fun t -> TaskUpdated (t.id, text))
                | RemoveTask id ->
                    state.tasks 
                    |> List.filter (fun t -> t.id = id) 
                    |> List.map (fun t -> TaskRemoved t.id)
                | Check id ->
                    state.tasks 
                    |> List.filter (fun t -> t.id = id && not t.isChecked) 
                    |> List.map (fun t -> Checked t.id)
                | Uncheck id ->
                    state.tasks 
                    |> List.filter (fun t -> t.id = id && t.isChecked) 
                    |> List.map (fun t -> Unchecked t.id)
                | RemoveAll ->
                    state.tasks 
                    |> List.map (fun t -> TaskRemoved t.id)
                | RemoveAllChecked ->
                    state.tasks 
                    |> List.filter (fun t -> t.isChecked) 
                    |> List.map (fun t -> TaskRemoved t.id)
                | CheckAll ->
                    state.tasks 
                    |> List.filter (fun t -> not t.isChecked) 
                    |> List.map (fun t -> Checked t.id)
                | UncheckAll ->
                    state.tasks 
                    |> List.filter (fun t -> t.isChecked) 
                    |> List.map (fun t -> Unchecked t.id)
//                
//                | _ -> 
//                    failProcessCommand astate command

let receiveEvent astate event =
    match astate with
        | None ->
            match event with
                | Created title ->
                    Some { title = title
                    ; nextTaskId = 1
                    ; tasks = [] 
                    }
//                
//                | _ -> raise (NotSupportedException "Cannot apply any event to non-existing aggregate")
                
        | Some state ->
            match event with
                | TitleUpdated newTitle -> 
                    Some { state with 
                            title = newTitle 
                    } 
                | TaskAdded (id, text) ->
                    Some { state with 
                            nextTaskId = state.nextTaskId + 1; 
                            tasks = state.tasks @ [ { id = id; text = text; isChecked = false } ] 
                    }
                | TaskUpdated (id, text) ->
                    let mapTask task = 
                        if task.id = id 
                        then { task with text = text } 
                        else task
                    Some { state with 
                            tasks = state.tasks |> List.map mapTask 
                    }
                | TaskRemoved id ->
                    Some { state with 
                            tasks = state.tasks |> List.filter (fun t -> t.id <> id)
                    }
                | Checked id ->
                    let mapTask task = 
                        if task.id = id && not task.isChecked 
                        then { task with isChecked = true } 
                        else task
                    Some { state with 
                            tasks = state.tasks |> List.map mapTask 
                    }
                | Unchecked id ->
                    let mapTask task = 
                        if task.id = id && task.isChecked 
                        then { task with isChecked = false } 
                        else task
                    Some { state with 
                            tasks = state.tasks |> List.map mapTask 
                    }
//                
//                | _ -> raise (NotSupportedException "Cannot apply Created event to an existing aggregate")

let validate command =
    let validateId id =
        seq {
            if id <= 0 
            then yield memberFailure "id" "Id must be positive"
        }

    let validateTitle title =
        seq {
            if String.IsNullOrEmpty title 
            then yield memberFailure "title" "Title cannot be empty"
            else if title.Length < 4 || title.Length > 100 
                then yield memberFailure "title" "Title length must be between 4 and 100"
        }

    let validateTaskText text =
        seq {
            if String.IsNullOrEmpty text 
            then yield memberFailure "text" "Task text cannot be empty"
            else if text.Length < 4 || text.Length > 100 
                then yield memberFailure "text" "Task text length must be between 4 and 100"
        }

    in
    match command with
        | Create title -> 
            validateTitle title
        | UpdateTitle title -> 
            validateTitle title
        | AddTask text -> 
            validateTaskText text
        | UpdateTask (id, text) ->
            seq { 
                yield! validateId id
                yield! validateTaskText text
            }
        | RemoveTask id -> 
            validateId id
        | Check id -> 
            validateId id
        | Uncheck id -> 
            validateId id
        | RemoveAll -> 
            Seq.empty
        | RemoveAllChecked -> 
            Seq.empty
        | CheckAll -> 
            Seq.empty
        | UncheckAll -> 
            Seq.empty
