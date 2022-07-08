// Tests various errors.
module TestError
open FarNet
open FarNet.FSharp

// Exception in a job function.
let workFuncError = async {
    do! job { failwith "demo-error" }
    Assert.Unexpected()
}

Test.Add("testFuncError", async {
    Jobs.StartImmediate workFuncError
    do! Assert.Wait(fun () ->
        Window.IsDialog()
        && far.Dialog[0].Text = "Exception"
        && far.Dialog[1].Text = "demo-error")
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

// Error due to an invalid macro.
let workMacroError = async {
    do! Jobs.Macro "bar"
    Assert.Unexpected()
}

Test.Add("testMacroError", async {
    // _201221_2o Keep starting this way, cover the bug.
    Jobs.StartImmediate workMacroError
    do! Assert.Wait(fun () ->
        Window.IsDialog()
        && far.Dialog[0].Text = "ArgumentException"
        && far.Dialog[3].Text = "Macro: bar (Parameter 'macro')"
    )
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})
