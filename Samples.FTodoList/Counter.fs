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

let initialState = 0

let act command value =
    match command with 
    | Increment -> [ WasIncremented ]
    | Decrement -> [ WasDecremented ]

let apply event value =
    match event with
    | WasIncremented -> value + 1
    | WasDecremented -> value - 1

let validate command : ValidationItem seq =
    match command with
    | Increment -> Seq.empty
    | Decrement -> Seq.empty

