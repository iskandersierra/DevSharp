namespace Samples.FTodoList.Tests

open System
open DevFSharp.Validations
open NUnit.Framework
open DevFSharp.NUnitTests.TestHelpers
open Samples.FTodoList.TodoList

[<TestFixture>]
type ``TodoList validate tests``() = 

    [<Test>] 
    member test.``validate Create with null title is invalid`` () = 
        testIsInvalidCommand validate (Create null)

    [<Test>] 
    member test.``validate Create with empty title is invalid`` () = 
        testIsInvalidCommand validate (Create "")

    [<Test>] 
    member test.``validate Create with short title is invalid`` () = 
        testIsInvalidCommand validate (Create (String ('a', 3)))

    [<Test>] 
    member test.``validate Create with long title is invalid`` () = 
        testIsInvalidCommand validate (Create (String ('a', 101)))

    [<Test>] 
    member test.``validate Create with normal title is valid`` () = 
        testIsValidCommand validate (Create defaultTitle)

    [<Test>] 
    member test.``validate UpdateTitle with null title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTitle null)

    [<Test>] 
    member test.``validate UpdateTitle with empty title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTitle "")

    [<Test>] 
    member test.``validate UpdateTitle with short title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTitle (String ('a', 3)))

    [<Test>] 
    member test.``validate UpdateTitle with long title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTitle (String ('a', 101)))

    [<Test>] 
    member test.``validate UpdateTitle with normal title is valid`` () = 
        testIsValidCommand validate (UpdateTitle defaultTitle)

    [<Test>] 
    member test.``validate AddTask with null title is invalid`` () = 
        testIsInvalidCommand validate (AddTask null)

    [<Test>] 
    member test.``validate AddTask with empty title is invalid`` () = 
        testIsInvalidCommand validate (AddTask "")

    [<Test>] 
    member test.``validate AddTask with short title is invalid`` () = 
        testIsInvalidCommand validate (AddTask (String ('a', 3)))

    [<Test>] 
    member test.``validate AddTask with long title is invalid`` () = 
        testIsInvalidCommand validate (AddTask (String ('a', 101)))

    [<Test>] 
    member test.``validate AddTask with normal title is valid`` () = 
        testIsValidCommand validate (AddTask defaultTitle)

    [<Test>] 
    member test.``validate UpdateTask with null title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTask (1, null))

    [<Test>] 
    member test.``validate UpdateTask with empty title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTask (1, ""))

    [<Test>] 
    member test.``validate UpdateTask with short title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTask (1, (String ('a', 3))))

    [<Test>] 
    member test.``validate UpdateTask with long title is invalid`` () = 
        testIsInvalidCommand validate (UpdateTask (1, (String ('a', 101))))

    [<Test>] 
    member test.``validate UpdateTask with non-possitive id is invalid`` () = 
        testIsInvalidCommand validate (UpdateTask (-1, defaultTitle))

    [<Test>] 
    member test.``validate UpdateTask with normal title is valid`` () = 
        testIsValidCommand validate (UpdateTask (1, defaultTitle))

    [<Test>] 
    member test.``validate RemoveTask with non-possitive id is invalid`` () = 
        testIsInvalidCommand validate (RemoveTask -1)

    [<Test>] 
    member test.``validate RemoveTask with normal title is valid`` () = 
        testIsValidCommand validate (RemoveTask 1)

    [<Test>] 
    member test.``validate Check with non-possitive id is invalid`` () = 
        testIsInvalidCommand validate (Check -1)

    [<Test>] 
    member test.``validate Check with normal title is valid`` () = 
        testIsValidCommand validate (Check 1)

    [<Test>] 
    member test.``validate Uncheck with non-possitive id is invalid`` () = 
        testIsInvalidCommand validate (Uncheck -1)

    [<Test>] 
    member test.``validate Uncheck with normal title is valid`` () = 
        testIsValidCommand validate (Uncheck 1)

    [<Test>] 
    member test.``validate RemoveAll is valid`` () = 
        testIsValidCommand validate RemoveAll

    [<Test>] 
    member test.``validate RemoveAllChecked is valid`` () = 
        testIsValidCommand validate RemoveAllChecked

    [<Test>] 
    member test.``validate CheckAll is valid`` () = 
        testIsValidCommand validate CheckAll

    [<Test>] 
    member test.``validate UncheckAll is valid`` () = 
        testIsValidCommand validate UncheckAll
