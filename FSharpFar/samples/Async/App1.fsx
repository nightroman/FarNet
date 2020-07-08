// Tests the main demo flow and other test scenarios.
// Testing is done by flows concurrent with samples.

open FarNet
open FarNet.FSharp
open System
open System.Diagnostics
open System.Reflection

async {
    // windows must be closed
    do! job { if far.Window.Count <> 2 then failwith "Close Far Manger internal windows before tests." }

    // init
    let sw = Stopwatch.StartNew()

    // test
    let tests = Test.GetTests()
    for test in tests do
        do! job { far.UI.WriteLine(sprintf "Test %s" test.Key, ConsoleColor.Cyan) }
        do! ((test.Value :?> PropertyInfo).GetValue(null) :?> Async<unit>)

    // done
    do! job { far.UI.WriteLine(sprintf "Done %i tests %O" tests.Count sw.Elapsed, ConsoleColor.Green) }

    // exit
    if Environment.GetEnvironmentVariable("QuitFarAfterTests") = "1" then
        do! job { far.Quit() }
}
|> Job.StartImmediate
