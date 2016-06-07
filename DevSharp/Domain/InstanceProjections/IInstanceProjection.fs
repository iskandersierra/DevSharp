namespace DevSharp.Domain.InstanceProjections

open FSharp.Core
open DevSharp.Messaging
open DevSharp.Validations

type EventType   = obj


type IInstanceProjection =

    abstract member selectId: EventType -> CommandRequest -> string option

    abstract member create: string -> EventType -> CommandRequest -> Map<string, obj> option

    abstract member update: Map<string, obj> -> EventType -> CommandRequest -> Map<string, obj> option

