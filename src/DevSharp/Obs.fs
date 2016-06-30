module Obs

open System
open System.Reactive
open System.Reactive.Linq
open FSharp.Core
open System.Collections.Generic

let subscribe (onNext: 'a -> unit) (obs: IObservable<'a>) = 
    obs.Subscribe (onNext)
let subscribeWithError (onNext: 'a -> unit) (onError: exn -> unit) (obs: IObservable<'a>) = 
    obs.Subscribe (onNext, onError)
let subscribeAll (onNext: 'a -> unit) (onError: exn -> unit) (onCompleted: unit -> unit) (obs: IObservable<'a>) = 
    obs.Subscribe (onNext, onError, onCompleted)
let subscribeEnd (onError: exn -> unit) (onCompleted: unit -> unit) (obs: IObservable<'a>) = 
    obs.Subscribe ((fun _ -> ()), onError, onCompleted)
let subscribeForSideEffects (obs: IObservable<'a>) = 
    obs.Subscribe ()

let subscribeSafe (onNext: 'a -> unit) (obs: IObservable<'a>) = 
    obs.SubscribeSafe (Observer.Create((fun a -> onNext a)))
let subscribeSafeWithError (onNext: 'a -> unit) (onError: exn -> unit) (obs: IObservable<'a>) = 
    obs.SubscribeSafe (Observer.Create((fun a -> onNext a), (fun e -> onError e)))
let subscribeSafeAll (onNext: 'a -> unit) (onError: exn -> unit) (onCompleted: unit -> unit) (obs: IObservable<'a>) = 
    obs.SubscribeSafe (Observer.Create((fun a -> onNext a), (fun e -> onError e), (fun () -> onCompleted ())))
let subscribeSafeEnd (onError: exn -> unit) (onCompleted: unit -> unit) (obs: IObservable<'a>) = 
    obs.SubscribeSafe (Observer.Create((fun _ -> ()), (fun e -> onError e), (fun () -> onCompleted ())))
let subscribeSafeForSideEffects (obs: IObservable<'a>) = 
    obs.SubscribeSafe (Observer.Create((fun _ -> ())))

let select selector obs = 
    Observable.Select (obs, (fun x -> selector x))
let selecti selector obs = 
    Observable.Select (obs, (fun (x, i) -> selector (x, i)))

let selectMany (selector: 'a -> IObservable<'b>) obs = 
    Observable.SelectMany (obs, (fun x -> selector x))
