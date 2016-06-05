[<DevSharp.Annotations.AggregateModule>]
module Samples.Domains.Echoer

type Event = 
    | WasEchoed of string

type Command = 
    | Echo of string

let init: obj = null

let act command = 
    match command with
    | Echo message -> Some [ WasEchoed message ]

let apply (event: Event) = 
    init


let sampleFunc () =
    let stringListOpt: string list option = None
    let objSeqOpt = stringListOpt |> Option.bind (fun list -> list |> List.map (fun s -> s :> obj) |> Some)
    ()

let sampleFunc2 (stringListOpt: string list option) =
    if stringListOpt.IsNone then None
    else
        let list = stringListOpt.Value
        let sequence = list |> List.toSeq |> Seq.map (fun s -> s :> obj)
        Some sequence

