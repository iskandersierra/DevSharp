[<DevSharp.Annotations.AggregateModule>]
module Samples.Domains.Counter

type Event = 
| WasIncremented
| WasDecremented

type Command = 
| Increment
| Decrement

let init = 0

let act command value = 
    match command with
    | Increment -> Some [ WasIncremented ]
    | Decrement -> 
        if value > 0 
        then Some [ WasDecremented ]
        else Some List.empty

let apply event value = 
    match event with
    | WasIncremented -> value + 1
    | WasDecremented -> value - 1
