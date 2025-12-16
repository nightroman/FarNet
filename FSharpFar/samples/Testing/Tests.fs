module Tests
open FarNet.FSharp
open Swensen.Unquote

// This is a typical synchronous test, `unit -> unit` module function.
Test.Add("testSync", fun () ->
    printfn "in testSync"

    let tests1 = Test.SyncTests
    test <@ 3 = tests1.Count @>
    test <@ tests1.ContainsKey("testSync") @>
    test <@ tests1.ContainsKey("syncWithParameter1") @>
    test <@ tests1.ContainsKey("syncWithParameter2") @>

    let tests2 = Test.AsyncTests
    test <@ 3 = tests2.Count @>
    test <@ tests2.ContainsKey("testAsync") @>
    test <@ tests2.ContainsKey("asyncWithParameter1") @>
    test <@ tests2.ContainsKey("asyncWithParameter2") @>
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
