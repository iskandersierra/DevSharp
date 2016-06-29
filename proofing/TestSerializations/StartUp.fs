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
// | None          = 2
// | Check         = 3

type InputArgs = 
    {
        target: BenchTarget option;
        size:   BenchSize option;
        kind:   BenchType option;
        repeat: int option;
    }

let printEnumNames<'a> sep =
    System.String.Join (sep, typedefof<'a> |> System.Enum.GetNames )

let parseEnum str =
    match Enum.TryParse<'e> str with
    | true, v -> Some v
    | false, _ -> None

let parseInt32 str =
    match Int32.TryParse str with
    | true, v -> Some v
    | false, _ -> None

let parseFirst parser argv =
    let candidates = argv 
                   |> Array.toSeq 
                   |> Seq.map parser 
                   |> Seq.collect (fun v -> match v with | Some a -> [a] | _ -> [])
                   |> Seq.toList
    match candidates with
    | a :: _ -> Some a
    | [] -> None


let parseArgs (argv: string[]) =
    if argv = null
    then None
    else 
        if argv.Length <= 4 then
            let target: BenchTarget option = parseFirst parseEnum argv
            let size: BenchSize option = parseFirst parseEnum argv
            let kind: BenchType option = parseFirst parseEnum argv
            let repeat = parseFirst parseInt32 argv
            Some { target = target; size = size; kind = kind; repeat = repeat; }
        else
            None

type serializeFunc = obj -> MemoryStream -> unit
type deserializeFunc = (MemoryStream -> Type -> obj)
type objectFactory = int -> obj

type DatasetInfo =
    DatasetInfo of Type * objectFactory

type BenchContext =
    {
        target:       BenchTarget;
        size:         BenchSize;
        kind:         BenchType;
        repeat:       int;
        initialGC:    int list
        profiler:     MiniProfiler
        serializer:   serializeFunc
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

let initializeBench target size kind repeat =
    let gc = [0..2] |> List.map GC.CollectionCount

    MiniProfiler.Settings.ProfilerProvider <- SingletonProfilerProvider() :> IProfilerProvider

    let serialize, deserialize = selectFunctions target
    let datasets = selectDataSetInfos size

    {
        target = target
        size = size
        kind = kind
        repeat = repeat
        initialGC = gc
        profiler = MiniProfiler.Start()
        serializer = serialize
        deserializer = deserialize
        datasetInfos = datasets
    }

let createDatasets context =
    let createDataset info =
        match info with
        | DatasetInfo (type', factory) -> 
            seq { 1..context.repeat }
            |> Seq.map factory
            |> Seq.toArray

    context.datasetInfos
    |> List.map createDataset


let startBench (args: InputArgs) =
    let defaultRepeat = 100000
    match args with
    | { target = Some t; size = Some s } ->
        let kind = match args.kind with | None -> BenchType.Both | Some k -> k
        let repeat = match args.repeat with | None -> defaultRepeat | Some r -> r
        printfn "Running benchmark with:"
        printfn "Target: %A" t
        printfn "Size:   %A" s
        printfn "Kind:   %A" kind
        printfn "Repeat: %d" repeat

        let context = initializeBench t s kind repeat

        let datasets = 
            use step = context.profiler.Step "create"
            in createDatasets context

        MiniProfiler.Stop()

        let gc2 = [0..2] |> List.map (fun i -> GC.CollectionCount(i) - (context.initialGC.Item i))
        printfn "GC collection counts: %A" gc2

        printfn "%s" (MiniProfiler.Current.RenderPlainText())

    | _ ->
        printfn "Running benchmark with:"
        printfn "Target: %A" (match args.target with | None -> (printEnumNames<BenchTarget> " and ") :> obj | Some t -> t :> obj)
        printfn "Size:   %A" (match args.size with | None -> (printEnumNames<BenchSize> " and ") :> obj | Some t -> t :> obj)
        printfn "Kind:   %A" BenchType.Both
        printfn "Repeat: %d" (match args.repeat with | None -> defaultRepeat | Some t -> t)
        

