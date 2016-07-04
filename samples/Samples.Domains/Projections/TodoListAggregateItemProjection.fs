[<DevSharp.Annotations.ViewProjectionModule>]
module TodoListAggregateItemProjection

open System
open System.Linq

open DevSharp.Common
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils
open DevSharp.Annotations

open Samples.Domains.TodoList


//type TodoList =
//    {
//        id:         AggregateId
//        title:      TodoListTitle
//    }    
//
//
//[<AggregateProjectionSelect>]
//let select (query: IQueryable<TodoList>) event = 
//    None
//
//
//[<AggregateProjectionApply>]
//let apply event doc request =
//    match (doc, event) with
//    | (None, WasCreated title) ->
//        Some { 
//            id = request.aggregate.aggregateId
//            title = title
//            tasks = []
//        }
//
//    | (Some state, TitleWasUpdated newTitle) -> 
//        Some { 
//        state with 
//            title = newTitle
//        } 
//
//    | (Some state, TaskWasAdded (id, text)) ->
//        Some { 
//        state with 
//            tasks = state.tasks @ 
//                    [ { id = id; text = text; isChecked = false } ] 
//        }
//
//    | (Some state, TaskWasUpdated (id, text)) ->
//        let updatedTask task = 
//            if task.id = id 
//            then { task with text = text } 
//            else task
//        Some { 
//        state with 
//            tasks = state.tasks |> List.map updatedTask 
//        }
//
//    | (Some state, TaskWasRemoved id) ->
//        let differentTask task = task.id <> id
//        Some { 
//        state with 
//            tasks = state.tasks |> List.filter differentTask
//        }
//
//    | (Some state, TaskWasChecked id) ->
//        let checkedTask task = 
//            if task.id = id && not task.isChecked 
//            then { task with isChecked = true } 
//            else task
//        Some { 
//        state with 
//            tasks = state.tasks 
//                    |> List.map checkedTask 
//        }
//
//    | (Some state, TaskWasUnchecked id) ->
//        let uncheckedTask task = 
//            if task.id = id && task.isChecked 
//            then { task with isChecked = false } 
//            else task
//        Some { 
//        state with 
//            tasks = state.tasks 
//                    |> List.map uncheckedTask 
//        }
//
//    | (_, _) ->
//        doc
