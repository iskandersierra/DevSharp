[<DevFSharp.Annotations.AggregateModule>]
module Samples.FTodoList.PingPong

open System
open DevFSharp.Validations
open DevFSharp.Annotations

type Event = 
    | WasPinged
    | WasPonged

type Command = 
    | Ping
    | Pong

let init: obj = null

let act command = 
    match command with
    | Ping -> [ WasPinged ]
    | Pong -> [ WasPonged ]
