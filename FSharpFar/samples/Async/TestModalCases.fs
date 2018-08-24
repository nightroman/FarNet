
module TestModalCases
open FarNet
open Async
open Test

let testDialogOverDialog = async {
    // dialog 1
    Job.func showWideDialog
    |> startJob
    do! test isWideDialog

    // dialog 2 on top of 1
    Job.func (fun () -> far.Message "testDialogOverDialog")
    |> startJob
    do! test (isDialogText 1 "testDialogOverDialog")
    do! Job.keys "Esc"

    // dialog 1
    do! test isWideDialog
    do! Job.keys "Esc"

    // done
    do! test isFarPanel
}

let testEditorOverDialog = async {
    // dialog
    fun () -> far.Message "testEditorOverDialog"
    |> Job.func
    |> startJob
    do! test isDialog

    // editor
    fun () ->
        let editor = far.CreateEditor ()
        editor.Title <- "testEditorOverDialog"
        editor.Open ()
    |> Job.func
    |> startJob
    do! test isEditor
    do! Job.keys "Esc"

    // dialog
    do! test isDialog
    do! Job.keys "Esc"

    // done
    do! test isFarPanel
}

let test = async {
    do! testDialogOverDialog
    do! testEditorOverDialog
}
