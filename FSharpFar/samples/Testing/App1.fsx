open FarNet.FSharp
open System.Reflection

// Get tests. This may be used by a custom test runner.
// In this example we just print available test names.

let tests = Test.GetTests(Assembly.GetExecutingAssembly())
printfn "Tests:"
tests.Keys |> Seq.iter (printfn "%s")

// Run tests defined in the executing assembly, i.e. the current F# session.
// It is more reliable to specify the assembly explicitly. But in some cases
// the parameter may be omitted, e.g. this command line seems to work fine:
// fs: FarNet.FSharp.Test.Run()

Test.Run(Assembly.GetExecutingAssembly())
