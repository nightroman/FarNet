module Tests
open FarNet
open FarNet.FSharp
open System

/// This is a typical synchronous test, `unit -> unit` module function.
[<Test>]
let functionInModule () =
    printfn "in functionInModule"
    let tests = Test.GetTests()
    Assert.Equal(5, tests.Count)
    Assert.True(tests.ContainsKey("App1.functionInScript"))
    Assert.True(tests.ContainsKey("Tests.functionInModule"))
    Assert.True(tests.ContainsKey("Tests.asyncInModule"))
    Assert.True(tests.ContainsKey("Tests+Type1.TestSync"))
    Assert.True(tests.ContainsKey("Tests+Type1.TestAsync"))

/// This is a typical asynchronous test, `Async<unit>` module value.
[<Test>]
let asyncInModule = async {
    do! job { far.UI.WriteLine "in asyncInModule" }
}

/// Use types for groups of tests with common initialization and disposal.
type Type1 () =
    // This is the common initialization.
    do
        printfn "create Type1"

    // This is the common disposal.
    interface IDisposable with
        member __.Dispose() =
            printfn "dispose Type1"

    [<Test>]
    member __.TestSync () =
        printfn "in Type1.TestSync"

    [<Test>]
    member __.TestAsync = async {
        do! job { far.UI.WriteLine "in Type1.TestAsync" }
    }
