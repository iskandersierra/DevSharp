namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type TableRecordProjectionModuleAttribute() =
    inherit Attribute()

