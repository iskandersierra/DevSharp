[<DevSharp.Annotations.AggregateModule>]
module Samples.Domains.Echoer

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
