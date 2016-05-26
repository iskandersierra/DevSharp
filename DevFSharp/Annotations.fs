namespace DevFSharp.Annotations

open System
open FSharp.Core

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateModuleAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateEventsAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateCommandsAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateStateAttribute() =
    inherit Attribute()


[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type ProcessAggregateCommandAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type ReceiveAggregateEventAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type ValidateAggregateCommandAttribute() =
    inherit Attribute()
