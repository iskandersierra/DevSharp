[<DevSharp.Annotations.AggregateModule>]
[<DevSharp.Annotations.DisplayName("Ping-Pong")>]
module Samples.Domains.PingPong

type Event = 
    | WasPinged
    | WasPonged

type Command = 
    | Ping
    | Pong

let act command = 
    match command with
    | Ping -> Some [ WasPinged ]
    | Pong -> Some [ WasPonged ]
