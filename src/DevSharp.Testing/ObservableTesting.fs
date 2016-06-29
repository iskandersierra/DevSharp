[<AutoOpen>]
module ObservableTesting

open FSharp.Core
open System
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open FsUnit
open NUnit.Framework
open NUnit.Framework.Constraints

let shouldProduceSingleAction (action: 'a -> unit) (obs: IObservable<'a>) =
    use waitHandle = new AutoResetEvent(false)
    let mutable produced = false
    use subscription = 
        obs.Subscribe(
                (fun v -> 
                    if produced 
                    then do Assert.Fail("Observable should produce a single value but produced many instead")
                    else do action v; do produced <- true),
                (fun (ex: exn) -> do Assert.Fail("Observable should return a value but failed with: " + ex.Message)), 
                (fun () -> 
                    if produced 
                    then do waitHandle.Set() |> ignore
                    else do Assert.Fail("Observable should produce a single value but produced none instead")))

    do waitHandle.WaitOne(500) |> should be True

let shouldProduceSingle (value: 'a) (obs: IObservable<'a>) = 
    shouldProduceSingleAction (should equal value) obs


let shouldProduceListAction (action: 'a list -> unit) (obs: IObservable<'a>) =
    shouldProduceSingleAction action (obs.ToList().Select(Seq.toList))

let shouldProduceList (list: 'a list) (obs: IObservable<'a>) = 
    shouldProduceListAction (should equal list) obs
