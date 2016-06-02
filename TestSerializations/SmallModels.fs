module SmallModels

open System

type MessageRec =
    {
        message: string;
        version: int;
    }
    with override this.ToString () = sprintf "%A" this

let messageRec msg ver = { message = msg; version = ver; }

type MessageSum =
| MessageSum of string * int

let messageSum msg ver = MessageSum (msg, ver)

type MessageTup = 
    string * int

let messageTup msg ver : MessageTup = (msg, ver)

let message id = messageTup (sprintf "Message #%A" id) id



type ComplexRec =
    {
        x: decimal;
        y: float;
        z: uint64;
    }

let complexRec x y z = { x = x; y = y; z = z; }

type ComplexSum =
    ComplexSum of decimal * float * uint64

let complexSum x y z = ComplexSum (x, y, z)

let complex id = complexSum (decimal id) (float id) (uint64 id)

type PostRec =
    {
        ID: System.Guid;
        title: string;
        active: bool;
        created: System.DateTime;
    }

let postRec id title active created = 
    { ID = id; title = title; active = active; created = created; }
    
let post id = postRec (Guid.NewGuid()) (sprintf "Post #%A" id) (id % 5 = 0) (DateTime.Today.AddMinutes (float id))







