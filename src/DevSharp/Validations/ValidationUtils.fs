module DevSharp.Validations.ValidationUtils

let validationResult items =
    let itemsList = items |> Seq.toList
    let isValidItem it =
        match it with 
        | Failure (_, _) -> false 
        | _ -> true
    let isValid = itemsList |> List.forall isValidItem
    {
        items = itemsList;
        isValid = isValid;
    }

let successResult = validationResult Seq.empty

let failure message = Failure (message, UnknownFailure)
let failureResult message = validationResult (seq { yield failure message })

let commandFailure message = Failure (message, CommandFailure)
let commandFailureResult message = validationResult (seq { yield commandFailure message })

let memberFailure ``member`` message = Failure (message, MemberFailure ``member``)
let memberFailureResult ``member`` message = validationResult (seq { yield memberFailure ``member`` message })

let exceptionFailure ex = 
    let ef = ExceptionFailure ex
    Failure (ex.Message, ef)
let exceptionFailureResult ex = validationResult (seq { yield exceptionFailure ex })
