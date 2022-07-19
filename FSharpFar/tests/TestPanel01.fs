module TestPanel01
open FarNet
open FarNet.FSharp

// Not a panel test but related to panels.
Test.Add("testSkipModal", async {
    async {
        // dialog
        Jobs.StartImmediate(Jobs.Job showWideDialog)
        // wait
        do! Jobs.WaitModeless()
        // done
        do! job { far.Message "done" }
    }
    |> Jobs.StartImmediate
    do! job { Assert.True(isWideDialog ()) }

    // exit dialog -> trigger "done" after waiting
    do! Jobs.Keys "Esc"
    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[1].Text = "done")

    // exit dialog
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

Test.Add("testCannotOpenOnModal", async {
    // dialog
    Jobs.StartImmediate(Jobs.Job showWideDialog)
    do! job { Assert.Dialog() }

    // try open panel from dialog -> job error dialog
    Jobs.StartImmediate <| Jobs.OpenPanel(MyPanel.panel [])
    do! Assert.Wait(fun () ->
        Window.IsDialog() &&
        far.Dialog[0].Text = "ModuleException" &&
        far.Dialog[1].Text = "Cannot open panel from modal window."
    )

    // exit two dialogs
    do! Jobs.Keys "Esc Esc"
    do! job { Assert.NativePanel() }
})

Test.Add("testCanOpenFromEditor", async {
    // editor
    do! job {
        let editor = far.CreateEditor(FileName = far.TempName())
        editor.DisableHistory <- true
        editor.Open()
    }
    do! job { Assert.Editor() }

    // panel
    do! Jobs.OpenPanel(MyPanel.panel [])
    do! job { Assert.True(isMyPanel ()) }

    // exit panel
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }

    // exit editor
    do! Jobs.Keys "F12 2 Esc"
    do! job {
        Assert.NativePanel()
        Assert.Equal(2, far.Window.Count)
    }
})
