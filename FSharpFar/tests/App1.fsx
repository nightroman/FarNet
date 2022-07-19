open FarNet
open FarNet.FSharp

// windows must be closed
if far.Window.Count <> 2 then failwith "Close Far Manager internal windows before tests."

// run added tests
Test.Run()
