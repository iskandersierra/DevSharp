module DevObservable

open System
open System.Reactive
open System.Reactive.Linq
open FSharp.Core
open System.Collections.Generic

let select selector obs = Observable.Select (obs, (fun x -> selector x))
let selecti selector obs = Observable.Select (obs, (fun (x, i) -> selector (x, i)))

let selectMany (selector: 'a -> IObservable<'b>) obs = Observable.SelectMany (obs, (fun x -> selector x))
