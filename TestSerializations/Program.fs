[<EntryPoint>]
let main argv = 
    let printHelp() =
        printfn "Expected usage:"
        printfn "TestSerializations.exe <Target> <Size> <Type> <Repeat>"
        printfn ""
        printfn "where"
        printfn "Target = %A" ( System.String.Join (" | ", typedefof<StartUp.BenchTarget> |> System.Enum.GetNames ))
        printfn "Size   = %A" ( System.String.Join (" | ", typedefof<StartUp.BenchSize>   |> System.Enum.GetNames ))
        printfn "Type   = %A" ( System.String.Join (" | ", typedefof<StartUp.BenchType>   |> System.Enum.GetNames ))
        printfn ""
        printfn "Example:"
        printfn "TestSerializations.exe JsonNet Small Both 1000000"
        ()
    let args = StartUp.parseArgs argv
    match args with
    | None -> printHelp()
    | Some a -> StartUp.startBench a
    0 
