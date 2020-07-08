open FarNet
open FarNet.FSharp
open System
open System.Diagnostics
open System.Reflection

// Tests are usually defined in modules (*.fs).
// But it is fine to define tests in a script.
[<Test>]
let functionInScript () =
    printfn "in functionInScript"

// get tests
let tests = Test.GetTests()
printfn "Tests:"
tests.Keys |> Seq.iter (printfn "%s")

// let's count tests and duration
let mutable tested = 0
let sw = Stopwatch.StartNew()

// Run synchronous tests.
// They are usually `unit -> unit` module functions or type methods.
printfn "Run sync tests..."
for test in tests do
    match test.Value with
    | :? MethodInfo as mi ->
        tested <- tested + 1
        printfn "Test %s" test.Key

        if mi.IsStatic then
            // module function or type static method
            mi.Invoke(null, null) |> ignore
        else
            // type method
            let instance = Activator.CreateInstance(mi.DeclaringType)
            mi.Invoke(instance, null) |> ignore
            match instance with
            | :? IDisposable as dispose ->
                dispose.Dispose()
            | _ ->
                ()
    | _ ->
        ()

// Run asynchronous tests.
// They are usually `Async<unit>` module values or type properties.
printfn "Run async tests..."
async {
    for test in tests do
        match test.Value with
        | :? PropertyInfo as pi ->
            tested <- tested + 1
            do! job { far.UI.WriteLine(sprintf "Test %s" test.Key) }

            if pi.GetGetMethod().IsStatic then
                // module value or type static property
                do! (pi.GetValue(null) :?> Async<unit>)
            else
                // type property
                let instance = Activator.CreateInstance(pi.DeclaringType)
                do! (pi.GetValue(instance) :?> Async<unit>)
                match instance with
                | :? IDisposable as dispose ->
                    dispose.Dispose()
                | _ ->
                    ()
        | _ ->
            ()

    // summary
    Assert.Equal(tests.Count, tested)
    do! job { far.UI.WriteLine(sprintf "Done %i tests %O" tested sw.Elapsed, ConsoleColor.Green) }

    // exit?
    if Environment.GetEnvironmentVariable("QuitFarAfterTests") = "1" then
        do! job { far.Quit() }
}
|> Job.StartImmediate
