namespace DevSharp.Annotations

open System

[<AttributeUsageAttribute(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
type DocumentProjectionModuleAttribute() =
    inherit Attribute()
