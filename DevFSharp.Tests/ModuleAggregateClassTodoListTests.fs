namespace DevFSharp.Domain

open System
open FSharp.Reflection
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``ModuleAggregateClass tests with TodoList``() = 

    let todoListModuleType = typedefof<Command>.DeclaringType
    let todoListClass = ModuleAggregateClass(todoListModuleType) :> IAggregateClass
    let todoListTitle = "TodoList title"
    let emptyState = Some {title = todoListTitle; nextTaskId = 1; tasks = []}

    [<Test>] 
    member test.``loading a ModuleAggregateClass with TodoList aggregate module definition do not fail`` () =
        Assert.That(todoListClass, Is.Not.Null)

    [<Test>] 
    member test.``validating a correct Create command should give a valid result`` () =
        let result = todoListClass.validate (Create "TodoList title")
        Assert.That(result, Is.Not.Null)
        Assert.That(result.isValid, Is.True)

    [<Test>] 
    member test.``validating an incorrect Create command should give an invalid result`` () =
        let result = todoListClass.validate (Create null)
        Assert.That(result, Is.Not.Null)
        Assert.That(result.isValid, Is.False)

    [<Test>] 
    member test.``acting on a Create command over None should return a WasCreated event`` () =
        let result = todoListClass.act (Create "TodoList title") None
        Assert.That(result, Is.EquivalentTo([ WasCreated "TodoList title" ]))

    [<Test>] 
    member test.``acting on a Create command over Some state should fail`` () =
        let state = Some { title = ""; nextTaskId = 1; tasks = [] }
        let call = fun () -> todoListClass.act (Create "TodoList title") state |> ignore
        Assert.That(call, Throws.TypeOf<MatchFailureException>())

    [<Test>] 
    member test.``acting on a UpdateTitle command over Some state should return a TitleWasUpdated event`` () =
        let state = Some { title = ""; nextTaskId = 1; tasks = [] }
        let result = todoListClass.act (UpdateTitle "TodoList title") state
        Assert.That(result, Is.EquivalentTo([ TitleWasUpdated "TodoList title" ]))

    [<Test>] 
    member test.``acting on a UpdateTitle command over None state should fail`` () =
        let call = fun () -> todoListClass.act (UpdateTitle "TodoList title") None |> ignore
        Assert.That(call, Throws.TypeOf<MatchFailureException>())

    [<Test>] 
    member test.``applying a WasCreated event over None should return Some state`` () =
        let result = todoListClass.apply (WasCreated "TodoList title") None
        Assert.That(result, Is.EqualTo(Some {title = "TodoList title"; nextTaskId = 1; tasks = []}))

    [<Test>] 
    member test.``applying a WasCreated event over Some state should fail`` () =
        let call = fun () -> todoListClass.apply (WasCreated "TodoList title") emptyState |> ignore
        Assert.That(call, Throws.TypeOf<MatchFailureException>())

    [<Test>] 
    member test.``applying a TitleWasUpdated event over Some state should return Some state`` () =
        let result = todoListClass.apply (TitleWasUpdated "New TodoList title") emptyState
        Assert.That(result, Is.EqualTo(Some {title = "New TodoList title"; nextTaskId = 1; tasks = []}))

    [<Test>] 
    member test.``applying a TitleWasUpdated event over None should fail`` () =
        let call = fun () -> todoListClass.apply (TitleWasUpdated "New TodoList title") None |> ignore
        Assert.That(call, Throws.TypeOf<MatchFailureException>())



