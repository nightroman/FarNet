
module TestFlowDialog
open FarNet
open FarNet.FSharp

let flow = async {
    // dialog
    let dialog = far.CreateDialog (-1, -1, 52, 3)
    let edit = dialog.AddComboBox (1, 1, 50, "")
    edit.Add "item1" |> ignore
    edit.Add "item2" |> ignore

    // flow, show the result text or "cancel"
    match! Job.FlowDialog (dialog, fun args ->
            if isNull args.Control then
                None
            elif edit.Text = "" then
                args.Ignore <- true
                None
            else
                Some edit.Text
        ) with
    | Some text ->
        do! job { far.Message text }
    | None ->
        do! job { far.Message "cancel" }
}

let testEmpty = async {
    Job.StartImmediate flow
    do! job { Assert.Dialog () }

    // enter empty text
    do! Job.Keys "Enter"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("", far.Dialog.[0].Text)
    }

    // cancel
    do! Job.Keys "Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("cancel", far.Dialog.[1].Text)
    }

    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

let testItem1 = async {
    Job.StartImmediate flow
    do! job { Assert.Dialog () }

    // enter the first item from the list
    do! Job.Keys "CtrlDown Enter Enter"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("item1", far.Dialog.[1].Text)
    }

    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

let test = async {
    do! testEmpty
    do! testItem1
}
