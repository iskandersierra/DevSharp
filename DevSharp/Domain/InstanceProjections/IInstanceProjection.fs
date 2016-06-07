namespace DevSharp.Domain.InstanceProjections

open FSharp.Core
open DevSharp
open DevSharp.Validations

type EventType   = obj


type IInstanceProjection =

    abstract member selectId: EventType -> CommandRequest -> string option

    abstract member create: string -> EventType -> CommandRequest -> PropertiesType option

    abstract member update: PropertiesType -> EventType -> CommandRequest -> PropertiesType option

