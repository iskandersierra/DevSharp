[<AutoOpen>]
module CommonTesting

open FSharp.Core
open System
open System.Threading
open FsUnit
open NUnit.Framework
open NUnit.Framework.Constraints

let failWithExn caption (ex: exn) = 
    do Assert.Fail(caption + ex.Message)

let resetAutoResetEvent (waitHandle: AutoResetEvent) () =
    do waitHandle.Set() |> ignore

let shouldBeSameList (expected: list<'a>) (current: list<'a>) =
    let expectedLength = expected |> List.length
    let currentLength = current |> List.length
    if expectedLength <> currentLength
    then Assert.Fail(sprintf "Current list do not have the same length than expected: %A but %A found" expected current)
    else do 
        expected 
        |> List.zip current 
        |> List.iteri 
            (fun i (e, c) -> 
                if e <> c 
                then Assert.Fail(sprintf "Current list differ at index %d with expected: %A but %A found" i e c))

let shouldBeSameSequence (expected: seq<'a>) (current: seq<'a>) =
    shouldBeSameList (expected |> Seq.toList) (current |> Seq.toList)