namespace DevFSharp.Domain

open System
open FSharp.Core
open DevFSharp.Validations

type CommandType = obj
type EventType   = obj
type StateType   = obj

type AggregateClass =

    abstract member validateCommand: CommandType -> ValidationResult

    abstract member processCommand: StateType option -> CommandType -> ValidationResult

    abstract member receiveEvent: StateType option -> EventType -> ValidationResult

//type ModuleAggregateClass(aggregateModule: Type)
    
