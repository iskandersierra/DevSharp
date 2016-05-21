module DevFSharp.Validations

open FSharp.Reflection

type ValidationFailure = 
    | Failure of string
    | MemberFailure of string * string
    | ExceptionFailure of string * System.Exception

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
