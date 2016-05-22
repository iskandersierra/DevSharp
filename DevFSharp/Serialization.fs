module DevFSharp.Serialization

open System
open System.Collections.Generic

type ItemType =
    | Command
    | Event
    | State

type SerializableItem = 
    {
        Namespace : string;
        ItemType : ItemType;
        Name : string;
        Version : string;
        Item: IDictionary<string, Object>
        Metadata: IDictionary<string, Object>
    }

