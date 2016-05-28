namespace DevSharp.Domain

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

    abstract member validate: CommandType -> Request -> ValidationResult

    abstract member act: CommandType -> StateType -> Request -> EventType seq

    abstract member apply: EventType -> StateType -> Request -> StateType
