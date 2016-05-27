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

let failure message = Failure (message, UnknownFailure)

let memberFailure ``member`` message = Failure (message, MemberFailure ``member``)

let exceptionFailure (ex: System.Exception) = Failure (ex.Message, ExceptionFailure ex)
