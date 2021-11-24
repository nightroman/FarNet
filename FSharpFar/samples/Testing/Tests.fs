module Tests
open FarNet.FSharp
open System

/// This is a typical synchronous test, `unit -> unit` module function.
[<Test>]
let testSync () =
    printfn "in testSync"
    let tests = Test.GetTests()
    Assert.Equal(6, tests.Count)
    Assert.True(tests.ContainsKey("Tests.testSync"))
    Assert.True(tests.ContainsKey("Tests.testAsync"))
    Assert.True(tests.ContainsKey("Tests.testWithParameter1"))
    Assert.True(tests.ContainsKey("Tests.testWithParameter2"))
    Assert.True(tests.ContainsKey("Tests+Type1.TestSync"))
    Assert.True(tests.ContainsKey("Tests+Type1.TestAsync"))

/// This is a typical asynchronous test, `Async<unit>` module value.
[<Test>]
let testAsync = async {
    do! job { printfn "in testAsync" }
}

/// Body of the test with parameters.
let testWithParameter x () =
    printfn "in testWithParameter x=%i" x

/// Test with parameter 1.
[<Test>]
let testWithParameter1 = testWithParameter 1

/// Test with parameter 2.
[<Test>]
let testWithParameter2 = testWithParameter 2

/// Use types for groups of tests with common initialization and disposal.
type Type1() =
    // To test initialization (increment) and disposal (decrement).
    // In each test x must be equal to 1.
    static let mutable x = 0

    // This is the common initialization.
    do
        // increment
        x <- x + 1

    // This is the common disposal.
    interface IDisposable with
        member __.Dispose() =
            // decrement
            x <- x - 1

    [<Test>]
    member __.TestSync() =
        Assert.Equal(1, x)
        printfn "in Type1.TestSync x=%i" x

    [<Test>]
    member __.TestAsync = async {
        Assert.Equal(1, x)
        do! job { printfn "in Type1.TestAsync x=%i" x }
    }
