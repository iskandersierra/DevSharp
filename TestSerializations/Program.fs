open System
open Samples.Models
open StartUp
open StackExchange.Profiling
open System.IO

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

let printHelp() =
    printfn "Expected usage:"
    printfn "TestSerializations.exe <Target> <Size> <Type> <Repeat>"
    printfn ""
    printfn "where"
    printfn "Target = %A" ( String.Join (" | ", typedefof<BenchTarget> |> Enum.GetNames ))
    printfn "Size   = %A" ( String.Join (" | ", typedefof<BenchSize> |> Enum.GetNames ))
    printfn "Type   = %A" ( String.Join (" | ", typedefof<BenchType> |> Enum.GetNames ))
    printfn ""
    printfn "Example:"
    printfn "TestSerializations.exe JsonNet Small Both 1000000"
    ()

type serializeFunc = (obj -> MemoryStream -> unit)
type deserializeFunc = (MemoryStream -> Type -> obj)

let selectFunctions target : serializeFunc * deserializeFunc =
    ((fun o s -> ()), (fun s t -> null))

let startBench args =
    let gc = [0..2] |> List.map GC.CollectionCount
    printfn "GC collection counts: %A" gc

    MiniProfiler.Settings.ProfilerProvider <- SingletonProfilerProvider() :> IProfilerProvider

    let profiler = MiniProfiler.Start();
    let serialize, deserialize = selectFunctions args.target

    let gc2 = [0..2] |> List.map (fun i -> GC.CollectionCount(i) - (gc.Item i))
    printfn "GC collection counts: %A" gc

[<EntryPoint>]
let main argv = 
    let args = parseArgs argv
    match args with
    | None -> printHelp()
    | Some a -> startBench a
    0 
