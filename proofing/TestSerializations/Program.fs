[<EntryPoint>]
let main argv = 
    let printHelp() =
        printfn "Expected usage:"
        printfn "TestSerializations.exe [<Target>] [<Size>] [<Type>] <Repeat>"
        printfn ""
        printfn "where"
        printfn "Target = %A" (StartUp.printEnumNames<StartUp.BenchTarget> " | ")
        printfn "Size   = %A" (StartUp.printEnumNames<StartUp.BenchSize> " | ")
        printfn "Type   = %A" (StartUp.printEnumNames<StartUp.BenchType> " | ")
        printfn ""
        printfn "Example:"
        printfn "TestSerializations.exe JsonNet Small Both 1000000"
        printfn "TestSerializations.exe JsonNet Small 1000000"
        ()
    let args = StartUp.parseArgs argv
    match args with
    | None -> printHelp()
    | Some a -> StartUp.startBench a
    0 
