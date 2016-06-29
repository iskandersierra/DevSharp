namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Property, Inherited = false, AllowMultiple = false)>]
type AggregateInitAttribute() =
    inherit Attribute()
