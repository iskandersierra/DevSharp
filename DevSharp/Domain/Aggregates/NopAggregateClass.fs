namespace DevSharp.Domain.Aggregates

open DevSharp.Validations
open DevSharp.Validations.ValidationUtils

type NopAggregateClass() =

    interface IAggregateClass with

        member this.init = null
        member this.isStateless = true
        
        member this.requiresRequest = false

        member this.validate command request = validationResult []

        member this.act command state request = None

        member this.apply event state request = null

    
