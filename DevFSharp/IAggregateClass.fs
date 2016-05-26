namespace DevFSharp.Domain

open FSharp.Core
open DevFSharp.Validations


type CommandType = obj
type EventType   = obj
type StateType   = obj


type IAggregateClass =

    abstract member validateCommand: CommandType -> ValidationResult

    abstract member initialState: StateType

    abstract member processCommand: StateType -> CommandType -> EventType seq

    abstract member receiveEvent: StateType -> EventType -> StateType


    
