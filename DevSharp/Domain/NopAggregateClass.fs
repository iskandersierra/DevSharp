namespace DevSharp.Domain

open DevSharp.Validations

type NopAggregateClass() =

    interface IAggregateClass with

        member this.validate command = validationResult []

        member this.init = null

        member this.act command state = Seq.empty

        member this.apply event state = null

    
