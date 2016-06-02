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
| Medium       = 1

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

type serializeFunc = obj -> MemoryStream -> unit
type deserializeFunc = (MemoryStream -> Type -> obj)
type objectFactory = int -> obj

type DatasetInfo =
    DatasetInfo of Type * objectFactory

type BenchContext =
    {
        args: InputArgs
        initialGC: int list
        profiler: MiniProfiler
        serializer: serializeFunc
        deserializer: deserializeFunc
        datasetInfos: DatasetInfo list
    }

let setupJsonNet () =
    let serializer = Newtonsoft.Json.JsonSerializer()
    serializer.TypeNameAssemblyFormat <- System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
    serializer.TypeNameHandling <- Newtonsoft.Json.TypeNameHandling.Auto
    let serialize (o: obj) (s: Stream) =
        use w = new System.IO.StreamWriter(s)
        use sw = new Newtonsoft.Json.JsonTextWriter(w)
        serializer.Serialize(sw, o)
        sw.Flush()
    let deserialize (s: Stream) t =
        use r = new System.IO.StreamReader(s)
        use sr = new Newtonsoft.Json.JsonTextReader(r)
        serializer.Deserialize(sr, t)
    (serialize, deserialize)

let selectFunctions target : serializeFunc * deserializeFunc =
    match target with
    | BenchTarget.JsonNet -> setupJsonNet ()
    | BenchTarget.MsgPack -> ((fun o s -> ()), (fun s t -> null))

let toObj (f: int -> 'a) =
    (fun id -> f id :> obj)

let selectDataSetInfos size =
    match size with
    | Small ->
        let ds1 = DatasetInfo(typedefof<SmallModels.MessageTup>, toObj SmallModels.message)
        let ds2 = DatasetInfo(typedefof<SmallModels.ComplexSum>, toObj SmallModels.complex)
        let ds3 = DatasetInfo(typedefof<SmallModels.PostRec>, toObj SmallModels.post)
        [ds1; ds2; ds3]
    | Medium ->
        let ds1 = DatasetInfo(typedefof<MediumModels.DeletePost>, toObj MediumModels.deletePost)
        let ds2 = DatasetInfo(typedefof<MediumModels.Post>, toObj MediumModels.post)
        [ds1; ds2]

let initializeBench args =
    let gc = [0..2] |> List.map GC.CollectionCount

    MiniProfiler.Settings.ProfilerProvider <- SingletonProfilerProvider() :> IProfilerProvider

    let serialize, deserialize = selectFunctions args.target
    let datasets = selectDataSetInfos args.size

    {
        args = args
        initialGC = gc
        profiler = MiniProfiler.Start()
        serializer = serialize
        deserializer = deserialize
        datasetInfos = datasets
    }

let createDatasets context =
    context.datasetInfos


let startBench args =
    let context = initializeBench args

    let datasets = createDatasets context

    let gc2 = [0..2] |> List.map (fun i -> GC.CollectionCount(i) - (context.initialGC.Item i))
    printfn "GC collection counts: %A" gc2

