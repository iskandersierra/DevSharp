module DevFSharp.Validations

open System
open FSharp.Reflection

type ValidationMessage  = string
type MemberName         = string

type ValidationItem = 
    | Valid
    | Information       of ValidationMessage
    | Warning           of ValidationMessage
    | Failure           of ValidationMessage * FailureType
and FailureType =
    | UnknownFailure
    | MemberFailure     of MemberName
    | ExceptionFailure  of Exception

type ValidationResult =
    {
        ValidationError : ValidationItem list;
        IsValid : bool;
    }

let failure message = Failure (message, UnknownFailure)

let memberFailure ``member`` message = Failure (message, MemberFailure ``member``)

let exceptionFailure (ex: System.Exception) = Failure (ex.Message, ExceptionFailure ex)

let messageForFailProcessCommand (astate: 'state option) command =
    let commandName = command.GetType().Name
    let stateName = 
        let stateType = typedefof<'state>
        let moduleName = stateType.DeclaringType.Name
        match astate with
            | None ->
                String.concat " " ["non-existing"; moduleName]
            | Some state ->
                if FSharpType.IsUnion stateType 
                    then String.concat " " ["existing"; moduleName; "in state"; state.GetType().Name]
                    else String.concat " " ["existing"; moduleName]
    in
    String.concat " " ["Cannot process command"; commandName; "for"; stateName]

let failProcessCommand (astate: 'state option) command =
    raise (System.NotSupportedException (messageForFailProcessCommand astate command))
