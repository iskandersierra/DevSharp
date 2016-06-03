namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateModuleAttribute() =
    inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class ||| AttributeTargets.Property, Inherited = true, AllowMultiple = false)>]
type DisplayNameAttribute(name: string) =
    inherit Attribute()
    member this.Name = name
