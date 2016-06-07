[<DevSharp.Annotations.InstanceProjectionModule>]
module Samples.Domains.TodoListRootProjection

open DevSharp.Messaging
open Samples.Domains.TodoList


type Instance =
    {
        id:         string;
        title:      TodoListTitle; 
        tasks:      TodoTask list;
    }    
and  TodoTask =
    { 
        id:        TaskId; 
        text:      TaskText; 
        isChecked: bool;
    }

let selectId (event: Event) (request: CommandRequest) : string option =
    request.instanceId

let create (id: string) (event: Event) (request: CommandRequest) : Instance option =
    match event with 
    | WasCreated title -> Some { id = id; title = title; tasks = [] }
    | _ -> None // do not create the record

let update (instance: Instance) (event: Event) (request: CommandRequest) : Instance option =
    match event with 
    | TitleWasUpdated title -> Some { instance with title = title; }
    | TaskWasAdded (id, text) -> 
        Some { 
        instance with 
            tasks = instance.tasks @ [ { id = id; text = text; isChecked = false } ] ; 
        }
    | TaskWasUpdated (id, text) -> 
        let updatedTask task = 
            if task.id = id 
            then { task with text = text } 
            else task
        Some { 
        instance with 
            tasks = instance.tasks |> List.map updatedTask 
        }
    | TaskWasRemoved id -> 
        let differentTask task = task.id <> id
        Some { 
        instance with 
            tasks = instance.tasks |> List.filter differentTask
        }
    | TaskWasChecked id -> 
        let checkedTask task = 
            if task.id = id && not task.isChecked 
            then { task with isChecked = true } 
            else task
        Some { 
        instance with 
            tasks = instance.tasks 
                    |> List.map checkedTask 
        }
    | TaskWasUnchecked id -> 
        let uncheckedTask task = 
            if task.id = id && task.isChecked 
            then { task with isChecked = false } 
            else task
        Some { 
        instance with 
            tasks = instance.tasks 
                    |> List.map uncheckedTask 
        }
    // | WasDeleted -> None
    | _ -> Some instance
        
