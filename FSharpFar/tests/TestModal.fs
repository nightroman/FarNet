module TestModal
open FarNet
open FarNet.FSharp
open Swensen.Unquote

Test.Add("testDialogOverDialog", async {
    // dialog 1
    do! run { far.Message "testDialogOverDialog_1" }
    do! job { test <@ "testDialogOverDialog_1" = far.Dialog[1].Text @> }

    // dialog 2 on top of 1
    do! run { far.Message "testDialogOverDialog_2" }
    do! job { test <@ "testDialogOverDialog_2" = far.Dialog[1].Text @> }

    // exit 2
    do! Jobs.Keys "Esc"
    do! job { test <@ "testDialogOverDialog_1" = far.Dialog[1].Text @> }

    // exit 1
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

Test.Add("testEditorOverDialog", async {
    // dialog
    do! run { far.Message "testEditorOverDialog" }
    do! job { test <@ "testEditorOverDialog" = far.Dialog[1].Text @> }

    // editor
    do! run {
        let editor = far.CreateEditor(FileName = far.TempName(), Title = "testEditorOverDialog")
        editor.DisableHistory <- true
        editor.Open()
    }
    do! job { Assert.Editor() }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.Dialog() }

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

Test.Add("testModalEditorIssue", async {
    // dialog
    do! Jobs.Run showWideDialog

    // editor with problems (cannot edit directory) over the dialog
    async {
        let editor = far.CreateEditor(FileName = __SOURCE_DIRECTORY__)
        editor.DisableHistory <- true
        do! Jobs.Editor editor
        Assert.Unexpected()
    }
    |> Jobs.StartImmediate

    // nasty Far message -> `wait`, not `test`
    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[1].Text = "It is impossible to edit the folder")
    do! Jobs.Keys "Esc"

    // posted error
    do! job {
        Assert.Dialog()
        test <@ "InvalidOperationException" = far.Dialog[0].Text @>
    }
    do! Jobs.Keys "Esc"

    // dialog before editor
    do! job { test <@ isWideDialog () @> }
    do! Jobs.Keys "Esc"

    do! job { Assert.NativePanel() }
})
