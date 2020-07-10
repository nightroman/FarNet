open FarNet.FSharp
open System.Reflection

// get tests, this may be used for a custom test runner
let tests = Test.GetTests(Assembly.GetExecutingAssembly())
printfn "Tests:"
tests.Keys |> Seq.iter (printfn "%s")

// run tests from the current assembly
Test.Run(Assembly.GetExecutingAssembly())
