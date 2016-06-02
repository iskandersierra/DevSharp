module SmallModels

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

type PostRec =
    {
        ID: System.Guid;
        title: string;
        active: bool;
        created: System.DateTime;
    }

let postRec id title active created = 
    { ID = id; title = title; active = active; created = created; }
    







