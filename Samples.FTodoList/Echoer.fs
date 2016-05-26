[<DevFSharp.Annotations.AggregateModule>]
module Samples.FTodoList.Echoer

open System
open DevFSharp.Validations
open DevFSharp.Annotations

type Event = 
    | WasEchoed of string

type Command = 
    | Echo of string

let init: obj = null

let act command = 
    match command with
    | Echo message -> [ WasEchoed message ]

let apply (event: Event) = 
    init
