module Samples.FTodoList.TodoList

type Event =
    | Created     of string
    | TaskAdded   of int * string
    | TaskUpdated of int * string
    | TaskRemoved of int
    | Checked     of int * bool

type Command =
    | Create      of string
    | AddTask     of string
    | UpdateTask  of int * string
    | RemoveTask  of int
    | RemoveAll
    | RemoveAllDone
    | Check       of int * bool
    | CheckAll    of bool

type TodoTask = { id : int; description: string; isDone: bool; }
type State = { title: string; nextId: int; tasks: TodoTask list; }

let whenStartCommand command = 
    match command with
    | Create title -> 
        [ Created (title) ]

    | _ -> 
        []

let whenCommand (command, state) =
    match command with
    | AddTask description -> 
        [ TaskAdded (state.nextId, description) ]

    | UpdateTask (id, description) -> 
        state.tasks 
        |> List.filter (fun t -> t.id = id && t.description <> description) 
        |> List.map (fun t -> TaskUpdated (id, description))

    | RemoveTask id -> 
        state.tasks 
        |> List.filter (fun t -> t.id = id)
        |> List.take 1
        |> List.map (fun t -> TaskRemoved (t.id))

    | RemoveAll -> 
        state.tasks 
        |> List.map (fun t -> TaskRemoved (t.id))

    | RemoveAllDone -> 
        state.tasks 
        |> List.filter (fun t -> t.isDone)
        |> List.map (fun t -> TaskRemoved (t.id))

    | Check (id, isDone) -> 
        state.tasks 
        |> List.filter (fun t -> t.id = id && t.isDone <> isDone)
        |> List.map (fun t -> Checked (t.id, isDone))

    | CheckAll isDone -> 
        state.tasks 
        |> List.filter (fun t -> t.isDone <> isDone)
        |> List.map (fun t -> Checked (t.id, isDone))

    | _ -> 
        []

let onStartEvent event = // Some ({ title = title; nextId = 1; tasks = [] })
    match event with
    | Created title ->
        Some ({ title = title; nextId = 1; tasks = [] })

    | _ ->
        None

let onEvent (event, state) = // Some ({ title = title; nextId = 1; tasks = [] })
    match event with
    | TaskAdded (id, description) ->
        let newTask = { id = id; description = description; isDone = false }
        Some ({ state with nextId = state.nextId + 1; tasks = state.tasks @ [ newTask ]})

    | TaskUpdated (id, description) ->
        let updateFunc = fun t -> if t.id = id then { t with description = description } else t
        Some ({ state with tasks = state.tasks |> List.map updateFunc })

    | TaskRemoved id ->
        let removeFunc = fun t -> t.id = id
        Some ({ state with tasks = state.tasks |> List.filter removeFunc })

    | Checked (id, isDone) ->
        let checkFunc = fun t -> if t.id = id then { t with isDone = isDone } else t
        Some ({ state with tasks = state.tasks |> List.map checkFunc })

    | _ ->
        None
