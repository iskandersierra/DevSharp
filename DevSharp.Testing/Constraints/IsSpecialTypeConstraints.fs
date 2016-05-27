namespace DevSharp.Testing.Constraints

open System
open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit
open FSharp.Reflection
open System.Reflection
open DevSharp.Validations
open DevSharp.Testing


[<AbstractClass>]
type IsSpecialTypeConstraintBase () =
    inherit Constraint()

    override this.ApplyTo<'T> (actual: 'T) =
        let atype = (actual :> obj) :?> Type
        let isSuccess =
            if atype <> null
            then this.Matches atype
            else failwith "This constraint only allow to check for System.Type instances"

        ConstraintResult (this, actual, isSuccess)
        
    abstract member Matches: Type -> bool
        

type IsRecordConstraint() =
    inherit IsSpecialTypeConstraintBase()

    do
        base.Description <- "record"

    override this.Matches atype =
        FSharpType.IsRecord atype


type IsUnionConstraint() =
    inherit IsSpecialTypeConstraintBase()

    do
        base.Description <- "union"

    override this.Matches atype =
        FSharpType.IsUnion atype


type IsFunctionConstraint() =
    inherit IsSpecialTypeConstraintBase()

    do
        base.Description <- "function"

    override this.Matches atype =
        FSharpType.IsFunction atype


type IsModuleConstraint() =
    inherit IsSpecialTypeConstraintBase()

    do
        base.Description <- "module"

    override this.Matches atype =
        FSharpType.IsModule atype


type IsTupleConstraint() =
    inherit IsSpecialTypeConstraintBase()

    do
        base.Description <- "tuple"

    override this.Matches atype =
        FSharpType.IsTuple atype

