
module TestModalCases
open FarNet
open FarNet.FSharp
open Test

let testDialogOverDialog = async {
    // dialog 1
    Job.StartImmediateFrom showWideDialog
    do! test isWideDialog

    // dialog 2 on top of 1
    job { far.Message "testDialogOverDialog" }
    |> Job.StartImmediate
    do! test (isDialogText 1 "testDialogOverDialog")
    do! Job.Keys "Esc"

    // dialog 1
    do! test isWideDialog
    do! Job.Keys "Esc"

    // done
    do! test isFarPanel
}

let testEditorOverDialog = async {
    // dialog
    job { far.Message "testEditorOverDialog" }
    |> Job.StartImmediate
    do! test isDialog

    // editor
    job {
        let editor = far.CreateEditor (FileName = far.TempName (), Title = "testEditorOverDialog")
        editor.DisableHistory <- true
        editor.Open ()
    }
    |> Job.StartImmediate
    do! test isEditor
    do! Job.Keys "Esc"

    // dialog
    do! test isDialog
    do! Job.Keys "Esc"

    // done
    do! test isFarPanel
}

let test = async {
    do! testDialogOverDialog
    do! testEditorOverDialog
}
