[<AutoOpen>]
module Samples.FTodoList.Tests.TodoListTestHelpers

open System
open Samples.FTodoList.TodoList
open DevFSharp.Validations
open NUnit.Framework

let initialState : State option = None
let defaultTitle = "Title of todo list"
let emptyState = Some { title = defaultTitle; nextTaskId = 1; tasks = [] }
let emptyStateTitle title = Some { title = title; nextTaskId = 1; tasks = [] }
let createState checks = 
    { 
        title = defaultTitle; 
        nextTaskId = (List.length checks) + 1; 
        tasks = checks 
            |> List.mapi (fun i isChecked -> 
                { 
                    id = i + 1; 
                    text = "task #" + (i + 1).ToString(); 
                    isChecked = isChecked 
                })
    }
