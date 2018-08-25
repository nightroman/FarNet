
module TestModalCases
open FarNet
open FarNet.FSharp
open Test

let testDialogOverDialog = async {
    // dialog 1
    Job.As showWideDialog
    |> Job.Start
    do! test isWideDialog

    // dialog 2 on top of 1
    Job.As (fun () -> far.Message "testDialogOverDialog")
    |> Job.Start
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
    fun () -> far.Message "testEditorOverDialog"
    |> Job.As
    |> Job.Start
    do! test isDialog

    // editor
    fun () ->
        let editor = far.CreateEditor (FileName = far.TempName (), Title = "testEditorOverDialog")
        editor.DisableHistory <- true
        editor.Open ()
    |> Job.As
    |> Job.Start
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
