[<DevSharp.Annotations.DocumentProjectionModule>]
module TodoListDocumentProjection

open DevSharp.Messaging
open Samples.Domains.TodoList


type Document =
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

let selectId (event: Event) (request: Request) : string option =
    request.instanceId

let create (id: string) (event: Event) (request: Request) : Document option =
    match event with 
    | WasCreated title -> Some { id = id; title = title; tasks = [] }
    | _ -> None // do not create the record

let update (document: Document) (event: Event) (request: Request) : Document option =
    match event with 
    | TitleWasUpdated title -> Some { document with title = title; }
    | TaskWasAdded (id, text) -> 
        Some { 
        document with 
            tasks = document.tasks @ [ { id = id; text = text; isChecked = false } ] ; 
        }
    | TaskWasUpdated (id, text) -> 
        let updatedTask task = 
            if task.id = id 
            then { task with text = text } 
            else task
        Some { 
        document with 
            tasks = document.tasks |> List.map updatedTask 
        }
    | TaskWasRemoved id -> 
        let differentTask task = task.id <> id
        Some { 
        document with 
            tasks = document.tasks |> List.filter differentTask
        }
    | TaskWasChecked id -> 
        let checkedTask task = 
            if task.id = id && not task.isChecked 
            then { task with isChecked = true } 
            else task
        Some { 
        document with 
            tasks = document.tasks 
                    |> List.map checkedTask 
        }
    | TaskWasUnchecked id -> 
        let uncheckedTask task = 
            if task.id = id && task.isChecked 
            then { task with isChecked = false } 
            else task
        Some { 
        document with 
            tasks = document.tasks 
                    |> List.map uncheckedTask 
        }
    // | WasDeleted -> None
    | _ -> Some document
        
