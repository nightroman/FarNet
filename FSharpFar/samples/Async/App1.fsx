// Tests the main demo flow and other test scenarios.
// Testing is done by flows concurrent with samples.

open FarNet
open FarNet.FSharp
open System.Reflection

// windows must be closed
if far.Window.Count <> 2 then failwith "Close Far Manger internal windows before tests."

// run tests from the current assembly
Test.Run(Assembly.GetExecutingAssembly())
