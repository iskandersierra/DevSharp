module DevSharp.Testing.DomainTesting

open System
open System.Linq
open System.Reflection
open System.Reactive
open System.Reactive.Linq
open FsUnit
open FSharp.Reflection
open NUnit.Framework

open DevSharp.Validations
open DevSharp.Testing
open DevSharp.Testing.Constraints
open DevSharp
open DevSharp.Domain.Aggregates.AggregateBehavior
open DevSharp.DataAccess

let aRecord   = IsRecordConstraint()
let aUnion    = IsUnionConstraint()
let aFunction = IsFunctionConstraint()
let aModule   = IsModuleConstraint()
let aTuple    = IsTupleConstraint()

let applyEvents init apply events =
    List.scan (fun s c -> apply c) init events |> List.last

let applyEvents2 init apply events =
    List.scan (fun s c -> apply c s) init events |> List.last

let applyEvents3 init apply events context =
    List.scan (fun s c -> apply c s context) init events |> List.last

let shouldBeAUnion (def: UnionDef) (atype: Type) =
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

    let matchingCases = actualCaseInfos |> List.except newCases |> List.map (fun info -> (info, def.cases |> List.find (fun c -> c.caseName = info.Name)))
    let rec checkMatching (pairs: (UnionCaseInfo * UnionCaseDef) list) =
        match pairs with
        | [] -> ()
        | ( info, case ) :: tail -> 
            let fieldTypes = info.GetFields() |> Array.toList |> List.map (fun p -> p.PropertyType)
            Enumerable.SequenceEqual (fieldTypes, case.types) |> should be True
            checkMatching tail

    checkMatching matchingCases
    

type TestingActionCall =
    | Success
    | Reject
    | Emit of EventType list * CommandRequest * IObservable<unit>
    | Error of ErrorResult
    | Postpone

type TestingActions() as this =
    inherit TestingMock<TestingActionCall>()
    
    interface IAggregateActorActions with
        member __.success ()                  = do this.add Success
        member __.reject result               = do this.add Reject
        member __.emit events request writing = do this.add (Emit (events, request, writing))
        member __.error kind msg              = do this.add (Error kind)
        member __.postpone ()                 = do this.add Postpone

type TestingEventStoreCall =
    | ReadCommit of ReadCommitsInput
    | WriteCommit of WriteCommitInput

type TestingEventStore(reader, writer) as this =
    inherit TestingMock<TestingEventStoreCall>()

    new (reader) = TestingEventStore(reader, (fun _ -> Observable.Empty()))
    new (writer) = TestingEventStore((fun _ -> Observable.Empty()), writer)

    interface IEventStoreReader with
        member __.readCommits input = 
            do this.add (ReadCommit input)
            reader input

    interface IEventStoreWriter with
        member __.writeCommit input = 
            do this.add (WriteCommit input)
            writer input
