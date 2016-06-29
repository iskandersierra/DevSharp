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