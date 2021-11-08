module TestDialog
open FarNet
open FarNet.FSharp

let work = async {
    // dialog
    let dialog = far.CreateDialog (-1, -1, 52, 3)
    let edit = dialog.AddComboBox (1, 1, 50, "")
    edit.Add "item1" |> ignore
    edit.Add "item2" |> ignore

    // show the result text or "cancel"
    match! Jobs.Dialog
        (dialog, fun args ->
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

[<Test>]
let testEmpty = async {
    Jobs.StartImmediate work
    do! job { Assert.Dialog () }

    // enter empty text
    do! Jobs.Keys "Enter"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("", far.Dialog[0].Text)
    }

    // cancel
    do! Jobs.Keys "Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("cancel", far.Dialog[1].Text)
    }

    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel () }
}

[<Test>]
let testItem1 = async {
    Jobs.StartImmediate work
    do! job { Assert.Dialog () }

    // enter the first item from the list
    do! Jobs.Keys "CtrlDown Enter Enter"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("item1", far.Dialog[1].Text)
    }

    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel () }
}
