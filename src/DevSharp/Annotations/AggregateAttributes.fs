namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Property, Inherited = false, AllowMultiple = false)>]
type AggregateInitAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateModuleAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class ||| AttributeTargets.Property, Inherited = true, AllowMultiple = false)>]
type DisplayNameAttribute(name: string) =
    inherit Attribute()
    member this.Name = name

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateEventAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateCommandAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateStateAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type AggregateActAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type AggregateApplyAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type AggregateValidateAttribute() = inherit Attribute()
