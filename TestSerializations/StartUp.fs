module StartUp

open System
open System.IO

open StackExchange.Profiling

open Samples.Models

type BenchTarget = 
| JsonNet        = 0
| MsgPack        = 1

type BenchSize =
| Small        = 0

type BenchType  =
| Serialization = 0
| Both          = 1
| None          = 2
| Check         = 3

type InputArgs = 
    {
        target: BenchTarget;
        size:   BenchSize;
        kind:   BenchType;
        repeat: int;
    }

let parseEnum str =
    match Enum.TryParse<'e> str with
    | true, v -> Some v
    | false, _ -> None

let parseInt32 str =
    match Int32.TryParse str with
    | true, v -> Some v
    | false, _ -> None

let parseArgs (argv: string[]) : InputArgs option =
    if argv = null || argv.Length <> 4 
    then None
    else
        let target: BenchTarget option = parseEnum argv.[0]
        let size: BenchSize option = parseEnum argv.[1]
        let kind: BenchType option = parseEnum argv.[2]
        let repeat = parseInt32 argv.[3]
        match (target, size, kind, repeat) with
        | Some t, Some s, Some k, Some r -> Some { target = t; size = s; kind = k; repeat = r; }
        | _ -> None

type serializeFunc = (obj -> MemoryStream -> unit)
type deserializeFunc = (MemoryStream -> Type -> obj)

type BenchContext =
    {
        args: InputArgs
        initialGC: int list
        profiler: MiniProfiler
        serializer: serializeFunc
        deserializer: deserializeFunc
    }

let 

let selectFunctions target : serializeFunc * deserializeFunc =
    match target with
    | BenchTarget.JsonNet -> 
        ((fun o s -> ()), (fun s t -> null))
    | BenchTarget.MsgPack -> 
        ((fun o s -> ()), (fun s t -> null))

let initializeBench args =
    let gc = [0..2] |> List.map GC.CollectionCount

    MiniProfiler.Settings.ProfilerProvider <- SingletonProfilerProvider() :> IProfilerProvider

    let serialize, deserialize = selectFunctions args.target

    {
        args = args
        initialGC = gc
        profiler = MiniProfiler.Start()
        serializer = serialize
        deserializer = deserialize
    }


let startBench args =
    let context = initializeBench args

    let gc2 = [0..2] |> List.map (fun i -> GC.CollectionCount(i) - (context.initialGC.Item i))
    printfn "GC collection counts: %A" gc2

