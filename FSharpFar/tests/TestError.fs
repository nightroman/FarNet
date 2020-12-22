/// Tests various errors in flows.
module TestError
open FarNet
open FarNet.FSharp

/// Exception in a job function.
let flowFuncError = async {
    do! job { failwith "demo-error" }
    Assert.Unexpected ()
}

[<Test>]
let testFuncError = async {
    Job.StartImmediate flowFuncError
    do! Assert.Wait (fun () ->
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

[<Test>]
let testMacroError = async {
    // _201221_2o Keep starting this way, cover the bug.
    Job.StartImmediate flowMacroError
    do! Assert.Wait (fun () ->
        Window.IsDialog ()
        && far.Dialog.[0].Text = "ArgumentException"
        && far.Dialog.[3].Text = "Macro: bar"
        && far.Dialog.[4].Text = "Parameter name: macro")
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}
