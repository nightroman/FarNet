
module TestModalEditorIssue
open FarNet
open FarNet.FSharp
open Test

let flow = async {
    // dialog
    Job.As showWideDialog
    |> Job.Start
    do! test isDialog

    // editor with problems (cannot edit directory) over the dialog
    let editor = far.CreateEditor (FileName = __SOURCE_DIRECTORY__)
    editor.DisableHistory <- true
    do! Job.FlowEditor editor
    failwith "unexpected"
}

let test = async {
    Job.Start flow

    // nasty Far message -> `wait`, not `test`
    do! wait (fun () -> isDialog () && dt 1 = "It is impossible to edit the folder")
    do! Job.Keys "Esc"

    // posted FarNet error
    do! test (isDialogText 0 "InvalidOperationException")
    do! Job.Keys "Esc"

    // dialog before editor
    do! test isWideDialog
    do! Job.Keys "Esc"

    // done
    do! test isFarPanel
}
