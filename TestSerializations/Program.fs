open System
open StartUp

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

[<EntryPoint>]
let main argv = 
    let args = parseArgs argv
    match args with
    | None -> printHelp()
    | Some a -> startBench a
    0 
