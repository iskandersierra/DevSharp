namespace DevSharp.Domain.Aggregates

open FSharp.Core
open DevSharp.Messaging
open DevSharp.Validations

type CommandType = obj
type EventType   = obj
type StateType   = obj


type IAggregateClass =

    abstract member init: StateType
    abstract member isStateless: bool

    abstract member requiresRequest: bool

    abstract member validate: CommandType -> CommandRequest -> ValidationResult

    abstract member act: CommandType -> StateType -> CommandRequest -> EventType seq option

    abstract member apply: EventType -> StateType -> CommandRequest -> StateType
