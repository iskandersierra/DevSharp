[<DevSharp.Annotations.InstanceProjectionModule>]
module Samples.Domains.TodoListListProjection

open DevSharp.Messaging
open Samples.Domains.TodoList


type Instance =
    {
        id: string;
        title: TodoListTitle;
    }

let selectId (event: Event) (request: CommandRequest) : string option =
    request.instanceId

let create (id: string) (event: Event) (request: CommandRequest) : Instance option =
    match event with 
    | WasCreated title -> Some { id = id; title = title; }
    | _ -> None // do not create the instance

let update (instance: Instance) (event: Event) (request: CommandRequest) : Instance option =
    match event with 
    | TitleWasUpdated title -> Some { instance with title = title; }
    // | WasDeleted -> None
    | _ -> Some instance
        
