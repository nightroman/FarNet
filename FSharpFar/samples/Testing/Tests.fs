module Tests
open FarNet.FSharp

// This is a typical synchronous test, `unit -> unit` module function.
Test.Add("testSync", fun () ->
    printfn "in testSync"

    let tests1 = Test.SyncTests
    Assert.Equal(3, tests1.Count)
    Assert.True(tests1.ContainsKey("testSync"))
    Assert.True(tests1.ContainsKey("syncWithParameter1"))
    Assert.True(tests1.ContainsKey("syncWithParameter2"))

    let tests2 = Test.AsyncTests
    Assert.Equal(3, tests2.Count)
    Assert.True(tests2.ContainsKey("testAsync"))
    Assert.True(tests2.ContainsKey("asyncWithParameter1"))
    Assert.True(tests2.ContainsKey("asyncWithParameter2"))
)

// Body of a test with parameters.
let syncWithParameter x () =
    printfn "in syncWithParameter x=%i" x

// Test with parameter 1.
Test.Add("syncWithParameter1", syncWithParameter 1)

// Test with parameter 2.
Test.Add("syncWithParameter2", syncWithParameter 2)

// This is a typical asynchronous test, `Async<unit>` module value.
Test.Add("testAsync", async {
    do! job { printfn "in testAsync" }
})

// Body of a test with parameters.
let asyncWithParameter x = async {
    printfn "in asyncWithParameter x=%i" x
}

// Test with parameter 1.
Test.Add("asyncWithParameter1", asyncWithParameter 1)

// Test with parameter 2.
Test.Add("asyncWithParameter2", asyncWithParameter 2)
