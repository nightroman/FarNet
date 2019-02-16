
module TestModalEditorIssue
open FarNet
open FarNet.FSharp
open Test

let flow = async {
    // dialog
    Job.StartImmediateFrom showWideDialog
    do! job { Assert.Dialog () }

    // editor with problems (cannot edit directory) over the dialog
    let editor = far.CreateEditor (FileName = __SOURCE_DIRECTORY__)
    editor.DisableHistory <- true
    do! Job.FlowEditor editor
    Assert.Unexpected ()
}

let test = async {
    Job.StartImmediate flow

    // nasty Far message -> `wait`, not `test`
    do! Job.Wait (fun () -> Window.IsDialog () && far.Dialog.[1].Text = "It is impossible to edit the folder")
    do! Job.Keys "Esc"

    // posted FarNet error
    do! job {
        Assert.Dialog ()
        Assert.Equal ("InvalidOperationException", far.Dialog.[0].Text)
    }
    do! Job.Keys "Esc"

    // dialog before editor
    do! job { Assert.True (isWideDialog ()) }
    do! Job.Keys "Esc"

    do! job { Assert.NativePanel () }
}
