namespace DevSharp.Domain

open FSharp.Core
open DevSharp.Validations


type CommandType = obj
type EventType   = obj
type StateType   = obj


type IAggregateClass =

    abstract member validate: CommandType -> ValidationResult

    abstract member init: StateType

    abstract member act: CommandType -> StateType -> EventType seq

    abstract member apply: EventType -> StateType -> StateType


type NopAggregateClass() =

    interface IAggregateClass with

        member this.validate command = validationResult []

        member this.init = null

        member this.act command state = Seq.empty

        member this.apply event state = null

    
