module DevSharp.Testing.DomainTesting

open System
open NUnit.Framework
open FsUnit
open FSharp.Reflection
open System.Reflection
open DevSharp.Validations
open DevSharp.Testing
open DevSharp.Testing.Constraints

let aRecord   = IsRecordConstraint()
let aUnion    = IsUnionConstraint()
let aFunction = IsFunctionConstraint()
let aModule   = IsModuleConstraint()
let aTuple    = IsTupleConstraint()

let shouldBeAUnion (def: UnionDef) (atype: Type) =
    let extractCaseName (c: UnionCaseInfo) = c.Name
    let extractCaseDefName ({ caseName = name }) = name
    let extractPropertyType (p: PropertyInfo) = p.PropertyType

    atype |> should be aUnion
    atype.Name |> should equal def.unionName

    let actualCaseInfos = FSharpType.GetUnionCases atype |> Array.toList
    let missingCases = def.cases |> List.filter (fun c -> actualCaseInfos |> List.forall (fun info -> info.Name <> c.caseName))

    match missingCases with
    | h :: _ -> failwithf "Union case %s is missing" h.caseName
    | _ -> ()

    let newCases = actualCaseInfos |> List.filter (fun info -> def.cases |> List.forall (fun c -> info.Name <> c.caseName))

    match newCases with
    | h :: _ -> failwithf "Union case %s should not be found" h.Name
    | _ -> ()

    

// Returns true iff atype is a union type and its cases match with given cases
//let haveUnionCases (expectedCases: UnionCaseDef list) (atype: Type) : bool =
//    let extractCaseName (c: UnionCaseInfo) = c.Name
//    let extractCaseDefName (UnionCaseDef(n, _)) = n
//    let extractPropertyType (p: PropertyInfo) = p.PropertyType
//    let areMatch (c: UnionCaseInfo) (e: UnionCaseDef) =
//        match e with
//        | UnionCaseDef (expectedName, expectedParameterTypes) ->
//            if not (c.Name = expectedName) then false
//            else 
//                let fieldTypes = c.GetFields() |> Array.toList |> List.map extractPropertyType
//                Enumerable.SequenceEqual (fieldTypes, expectedParameterTypes)
//
//    if not (FSharpType.IsUnion(atype)) then false
//    else
//        let caseInfos = FSharpType.GetUnionCases atype |> Array.toList
//        if caseInfos.Length <> expectedCases.Length then false
//        else
//            let sortedCaseInfos = caseInfos |> List.sortBy extractCaseName
//            let sortedExpected = expectedCases |> List.sortBy extractCaseDefName
//                        
//            let areSame = List.forall2 areMatch sortedCaseInfos sortedExpected
//            areSame
//
//let haveExpectedUnionCases (expectedCases: UnionCaseDef list) : ExpectedUnionCasesConstraint =
//    ExpectedUnionCasesConstraint expectedCases