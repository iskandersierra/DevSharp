namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type AggregateInitAttribute() =
    inherit Attribute()
