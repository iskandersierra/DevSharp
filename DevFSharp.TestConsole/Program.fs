
open System
open DevFSharp.Validations
open DevFSharp.Serialization
open Samples.FTodoList.TodoList
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FSharp.Reflection

let sampleCommand = 
    Create "A todo list"

let sampleState =
    {
        title = "TodoList title";
        nextTaskId = 3;
        tasks = 
        [
            { id = 1; text = "task #1"; isChecked = true };
            { id = 2; text = "task #2"; isChecked = false };
            { id = 3; text = "task #3"; isChecked = true };
        ]
    }    

let printSimpleTests () =
    let cmd = sampleCommand
    let json = JsonConvert.SerializeObject(cmd)
    let cmd2 = JsonConvert.DeserializeObject<Command>(json)

    let state = sampleState
    let json2 = JsonConvert.SerializeObject(state)
    let state2 = JsonConvert.DeserializeObject<State>(json2)

    printfn "Command : %A" cmd
    printfn "JSON    : %s" json
    printfn "Command2: %A" cmd2
    printfn "Same    : %A" (cmd = cmd2)
    printfn ""
    printfn "State   : %A" state
    printfn "JSON    : %s" json2
    printfn "State2  : %A" state2
    printfn "Same    : %A" (state = state2)

type SerializedObject =
    {
        format: String;
        model: String;
        data: String;
    }

let getSerializableType obj =
    let objType = 
        obj.GetType()
    in
    if FSharpType.IsUnion(objType) 
    then objType.DeclaringType.FullName
    else objType.FullName

let serializeJson obj =
    let data = JsonConvert.SerializeObject (obj)
    data

let deserializeJson (type': Type) data =
    let obj = JsonConvert.DeserializeObject(data, type')
    obj

let testSerialization () =
    let cmd = sampleCommand
    let sCmd = serializeJson cmd
    let cmdType = getSerializableType cmd
    let cmd2 = deserializeJson typedefof<Command> sCmd

    let state = sampleState
    let sState = serializeJson state
    let stateType = getSerializableType state
    let state2 = deserializeJson typedefof<State> sState

    let tuple = (1, "abc", true)
    let sTuple = serializeJson tuple
    let tupleType = getSerializableType tuple
    let tuple2 = deserializeJson (tuple.GetType()) sTuple
    
    printfn "%A" cmd
    printfn "%s" sCmd
    printfn "%A" cmd2
    printfn "%A" (obj.Equals(cmd, cmd2))
    printfn "%s" cmdType
    printfn ""
    printfn "%A" state
    printfn "%s" sState
    printfn "%A" state2
    printfn "%A" (obj.Equals(state, state2))
    printfn "%s" stateType
    printfn ""
    printfn "%A" tuple
    printfn "%s" sTuple
    printfn "%A" tuple2
    printfn "%A" (obj.Equals(tuple, tuple2))
    printfn "%s" tupleType
    
let testSerializationPerformance () =
    let times = 1000000

    //let myobj = sampleCommand
    let myobj = sampleState
    let startTimeCmd = DateTime.Now
    let sCmd = [ for i in 0..times -> serializeJson myobj ]
    let endTimeCmd = DateTime.Now
    let elapsedCmd = endTimeCmd - startTimeCmd
    
    printfn "Serialize union: %fms per op" (elapsedCmd.TotalMilliseconds / float(times))


[<EntryPoint>]
let main argv = 
    // printSimpleTests

    //testSerialization()
    testSerializationPerformance()

    0 // return an integer exit code
