[<DevSharp.Annotations.TableRecordProjectionModule>]
module Samples.Domains.TodoListListProjection

open DevSharp.Messaging
open Samples.Domains.TodoList


type Record =
    {
        id: string;
        title: TodoListTitle;
    }

let selectId (event: Event) (request: Request) : string option =
    request.instanceId

let create (id: string) (event: Event) (request: Request) : Record option =
    match event with 
    | WasCreated title -> Some { id = id; title = title; }
    | _ -> None // do not create the record

let update (record: Record) (event: Event) (request: Request) : Record option =
    match event with 
    | TitleWasUpdated title -> Some { record with title = title; }
    // | WasDeleted -> None
    | _ -> Some record
        
