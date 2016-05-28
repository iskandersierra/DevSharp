namespace DevSharp.Messaging

open System


type Request (properties: Map<string, obj>) =
    
    member this.properties = 
        properties

