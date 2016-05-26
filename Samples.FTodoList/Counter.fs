[<DevFSharp.Annotations.AggregateModule>]
module Samples.FTodoList.Counter

open System
open DevFSharp.Validations
open DevFSharp.Annotations

type Event = 
    | WasIncremented
    | WasDecremented

type Command = 
    | Increment
    | Decrement

let init = 0

let act command value = 
    match command with
    | Increment -> [ WasIncremented ]
    | Decrement -> [ WasDecremented ]

let apply event value = 
    match event with
    | WasIncremented -> value + 1
    | WasDecremented -> value - 1
