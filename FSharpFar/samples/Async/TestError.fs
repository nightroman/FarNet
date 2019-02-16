/// Tests various errors in flows.
module TestError
open FarNet
open FarNet.FSharp

/// Exception in a job function.
let flowFuncError = async {
    do! job { failwith "demo-error" }
    Assert.Unexpected ()
}
let testFuncError = async {
    Job.StartImmediate flowFuncError
    do! Job.Wait (fun () ->
        Window.IsDialog ()
        && far.Dialog.[0].Text = "Exception"
        && far.Dialog.[1].Text = "demo-error")
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// Error due to an invalid macro.
let flowMacroError = async {
    do! Job.Macro "bar"
    Assert.Unexpected ()
}
let testMacroError = async {
    Job.StartImmediate flowMacroError
    do! Job.Wait (fun () ->
        Window.IsDialog ()
        && far.Dialog.[0].Text = "ArgumentException"
        && far.Dialog.[3].Text = "Macro: bar"
        && far.Dialog.[4].Text = "Parameter name: macro")
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

let test = async {
    do! testFuncError
    do! testMacroError
}
