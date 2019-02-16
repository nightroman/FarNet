
module TestModalCases
open FarNet
open FarNet.FSharp
open Test

let testDialogOverDialog = async {
    // dialog 1
    Job.StartImmediateFrom showWideDialog
    do! job { Assert.True (isWideDialog ()) }

    // dialog 2 on top of 1
    Job.StartImmediate (job { far.Message "testDialogOverDialog" })
    do! job {
        Assert.Dialog ()
        Assert.Equal ("testDialogOverDialog", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"

    // dialog 1
    do! job { Assert.True (isWideDialog ()) }
    do! Job.Keys "Esc"

    do! job { Assert.NativePanel () }
}

let testEditorOverDialog = async {
    // dialog
    Job.StartImmediate (job { far.Message "testEditorOverDialog" })
    do! job { Assert.Dialog () }

    // editor
    job {
        let editor = far.CreateEditor (FileName = far.TempName (), Title = "testEditorOverDialog")
        editor.DisableHistory <- true
        editor.Open ()
    }
    |> Job.StartImmediate
    do! job { Assert.Editor () }
    do! Job.Keys "Esc"

    // dialog
    do! job { Assert.Dialog () }
    do! Job.Keys "Esc"

    do! job { Assert.NativePanel () }
}

let test = async {
    do! testDialogOverDialog
    do! testEditorOverDialog
}
