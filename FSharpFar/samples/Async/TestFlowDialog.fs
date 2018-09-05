
module TestFlowDialog
open FarNet
open FarNet.FSharp
open Test

let flow = async {
    // dialog
    let dialog = far.CreateDialog (-1, -1, 52, 3)
    let edit = dialog.AddComboBox (1, 1, 50, "")
    edit.Add "item1" |> ignore
    edit.Add "item2" |> ignore

    // flow
    let! r = Job.FlowDialog dialog (fun args ->
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
        do! job { far.Message text }
    | None ->
        do! job { far.Message "cancel" }
}

let testEmpty = async {
    Job.StartImmediate flow
    do! test isDialog

    // enter empty text
    do! Job.Keys "Enter"
    do! test (isDialogText 0 "")

    // cancel
    do! Job.Keys "Esc"
    do! test (isDialogText 1 "cancel")

    do! Job.Keys "Esc"
    do! test isFarPanel
}

let testItem1 = async {
    Job.StartImmediate flow
    do! test isDialog

    // enter the first item from the list
    do! Job.Keys "CtrlDown Enter Enter"
    do! test (isDialogText 1 "item1")

    do! Job.Keys "Esc"
    do! test isFarPanel
}

let test = async {
    do! testEmpty
    do! testItem1
}
