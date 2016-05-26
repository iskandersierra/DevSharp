module DevFSharp.Validations

open System
open FSharp.Reflection

type ValidationMessage  = string
type MemberName         = string

type ValidationResult = 
    | Valid
    | Failure           of ValidationMessage
    | MemberFailure     of ValidationMessage * MemberName
    | ExceptionFailure  of ValidationMessage * Exception

type ValidationError =
    ValidationError     of ValidationResult list

let failure message = Failure message

let memberFailure ``member`` message = MemberFailure (message, ``member``)

let exceptionFailure (ex: System.Exception) = ExceptionFailure (ex.Message, ex)

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
