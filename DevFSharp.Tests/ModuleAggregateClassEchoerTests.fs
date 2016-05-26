namespace DevFSharp.Domain

open System
open FSharp.Reflection
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.Echoer

[<TestFixture>]
type ``ModuleAggregateClass tests with Echoer``() = 

    let echoerModuleType = typedefof<Command>.DeclaringType
    let mutable echoerClass = NopAggregateClass() :> IAggregateClass
    let message = "Helloooo!"
    
    [<SetUp>]
    member test.SetUpMethod () =
        echoerClass <- ModuleAggregateClass(echoerModuleType) :> IAggregateClass

    [<Test>] 
    member test.``loading a ModuleAggregateClass with Echoer aggregate module definition do not fail`` () =
        Assert.That(echoerClass, Is.Not.Null)

    [<Test>] 
    member test.``validating a correct Echo command should give a valid result`` () =
        let result = echoerClass.validate (Echo message)
        Assert.That(result, Is.Not.Null)
        Assert.That(result.isValid, Is.True)

    [<Test>] 
    member test.``validating a message-less Echo command should give a valid result`` () =
        let result = echoerClass.validate (Echo null)
        Assert.That(result, Is.Not.Null)
        Assert.That(result.isValid, Is.True)

    [<Test>] 
    member test.``acting on a Echo command over some state should return a WasEchod event`` () =
        let result = echoerClass.act (Echo message) init
        Assert.That(result, Is.EquivalentTo([ WasEchoed message ]))

    [<Test>] 
    member test.``applying a WasEchoed event over any state should return the initial state`` () =
        let result = echoerClass.apply (WasEchoed message) init
        Assert.That(result, Is.EqualTo init)
