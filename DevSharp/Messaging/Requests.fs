namespace DevSharp.Messaging

open System


type CommandRequest (properties: Map<string, obj>) =
    
    let getProperty key =
        match Map.tryFind key properties with
        | None -> None
        | Some str -> Some (str :?> 'a)

    member this.properties = 
        properties

    member this.instanceId : string option = 
        getProperty "Id"

