namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type AggregateProjectionModuleAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type ViewProjectionModuleAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Property, Inherited = false, AllowMultiple = false)>]
type AggregateProjectionInitAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Property, Inherited = false, AllowMultiple = false)>]
type AggregateProjectionSelectAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
type AggregateProjectionApplyAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type RecordProjectionModuleAttribute() = inherit Attribute()

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type MultiRecordProjectionModuleAttribute() = inherit Attribute()

