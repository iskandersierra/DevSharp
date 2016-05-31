[<DevSharp.Annotations.AggregateModule>]
module Samples.Domains.PingPong

type Event = 
    | WasPinged
    | WasPonged

type Command = 
    | Ping
    | Pong

let act command = 
    match command with
    | Ping -> [ WasPinged ]
    | Pong -> [ WasPonged ]
