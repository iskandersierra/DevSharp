module DevFSharp.ReflectionUtils

open System
open System.Reflection

let typeOfFSharpList = typedefof<FSharp.Collections.List<_>>
let typeOfFSharpOption = typedefof<FSharp.Core.Option<_>>
let typeOfSeq = typedefof<System.Collections.Generic.IEnumerable<_>>
let typeOfEnumerable = typedefof<System.Linq.Enumerable>

let methodEnumerableCast = typeOfEnumerable.GetMethod("Cast")

let findModuleInnerType (moduleType: Type) containerName attrType defaultName =
    let isTheType (t: Type) =
        let attr = t.GetCustomAttributes(attrType, false)
        attr.Length > 0 || t.Name = defaultName

    let candidates = moduleType.GetNestedTypes(BindingFlags.Public) 
                    |> Array.toList
                    |> List.filter isTheType

    if candidates.Length = 1
    then candidates.Head
    else if candidates.Length > 1
            then raise (TypeLoadException (sprintf "Too many types found for %O on %O (%A)" defaultName containerName candidates))
            else raise (TypeLoadException (sprintf "Could not found type %O on %O" defaultName containerName))


let findModuleProperty (moduleType: Type) containerName attrType defaultName =
    let isTheProperty (p: PropertyInfo) =
        let attr = p.GetCustomAttributes(attrType, false)
        attr.Length > 0 || p.Name = defaultName

    let candidates = moduleType.GetProperties(BindingFlags.Static ||| BindingFlags.Public) 
                    |> Array.toList
                    |> List.filter isTheProperty

    if candidates.Length = 1
    then candidates.Head
    else if candidates.Length > 1
            then raise (TypeLoadException (sprintf "Too many properties found for %O on %O" defaultName containerName))
            else raise (TypeLoadException (sprintf "Could not found property %O on %O" defaultName containerName))
    
    
let findModuleMethod (moduleType: Type) containerName attrType defaultName (returnType: Type) (paramTypes: Type list) = 
    let isTheMethod (m: MethodInfo) =
        let attr = m.GetCustomAttributes(attrType, false)
        let isMaybe = attr.Length > 0 || m.Name = defaultName
        let parameters = m.GetParameters() 
                        |> Array.toList 
                        |> List.map (fun p -> p.ParameterType)
        isMaybe && 
            m.ReturnType = returnType && 
            System.Linq.Enumerable.SequenceEqual (paramTypes, parameters)

    let methods = moduleType.GetMethods(BindingFlags.Static ||| BindingFlags.Public)
    let candidates = methods 
                    |> Array.toList
                    |> List.filter isTheMethod

    if candidates.Length = 1
    then candidates.Head
    else if candidates.Length > 1
            then raise (TypeLoadException (sprintf "Too many methods found for %O on %O" defaultName containerName))
            else raise (TypeLoadException (sprintf "Could not found method %O on %O" defaultName containerName))

let getListType (elementType: Type) =
    typeOfFSharpList.MakeGenericType elementType

let getOptionType (elementType: Type) =
    typeOfFSharpOption.MakeGenericType elementType

let getSeqType (elementType: Type) =
    typeOfSeq.MakeGenericType elementType

let getMethodEnumerableCast (elementType: Type) =
    methodEnumerableCast.MakeGenericMethod elementType