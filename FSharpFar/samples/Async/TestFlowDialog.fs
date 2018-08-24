
module TestFlowDialog
open FarNet
open Async
open Test

let flow = async {
    // dialog
    let dialog = far.CreateDialog (-1, -1, 52, 3)
    let edit = dialog.AddComboBox (1, 1, 50, "")
    edit.Add "item1" |> ignore
    edit.Add "item2" |> ignore

    // flow
    let! r = Job.flowDialog dialog (fun args ->
        if isNull args.Control then
            None
        elif edit.Text = "" then
            args.Ignore <- true
            None
        else
            Some edit.Text
    )

    // show the result text or "cancel"
    match r with
    | Some text ->
        do! Job.func (fun () -> far.Message text)
    | None ->
        do! Job.func (fun () -> far.Message "cancel")
}

let testEmpty = async {
    startJob flow
    do! test isDialog

    // enter empty text
    do! Job.keys "Enter"
    do! test (isDialogText 0 "")

    // cancel
    do! Job.keys "Esc"
    do! test (isDialogText 1 "cancel")

    do! Job.keys "Esc"
    do! test isFarPanel
}

let testItem1 = async {
    startJob flow
    do! test isDialog

    // enter the first item from the list
    do! Job.keys "CtrlDown Enter Enter"
    do! test (isDialogText 1 "item1")

    do! Job.keys "Esc"
    do! test isFarPanel
}

let test = async {
    do! testEmpty
    do! testItem1
}
