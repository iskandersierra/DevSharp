namespace DevSharp.Validations

open System

type ValidationMessage  = string
type MemberName         = string

type ValidationItem = 
| Valid
| Information       of ValidationMessage
| Warning           of ValidationMessage
| Failure           of ValidationMessage * FailureType
and FailureType =
| UnknownFailure
| CommandFailure
| MemberFailure     of MemberName
| ExceptionFailure  of Exception

type ValidationResult =
    {
        items : ValidationItem list;
        isValid : bool;
    }
with 
    static member create items = 
        { 
            items = items
            isValid = items 
            |> List.forall (function | Valid | Information _ -> true | _ -> false ) }

exception ValidationException of ValidationResult