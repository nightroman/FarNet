
module TestModalEditorIssue
open FarNet
open Async
open Test

let flow = async {
    // dialog
    Job.func showWideDialog
    |> startJob
    do! test isDialog

    // editor with problems over the dialog
    let editor = far.CreateEditor ()
    editor.FileName <- __SOURCE_DIRECTORY__
    do! Job.flowEditor editor
    failwith "unexpected"
}

let test = async {
    startJob flow

    // nasty Far message -> `wait`, not `test`
    do! wait (fun () -> isDialog () && dt 1 = "It is impossible to edit the folder")
    do! Job.keys "Esc"

    // posted FarNet error
    do! test (isDialogText 0 "InvalidOperationException")
    do! Job.keys "Esc"

    // dialog before editor
    do! test isWideDialog
    do! Job.keys "Esc"

    // done
    do! test isFarPanel
}
