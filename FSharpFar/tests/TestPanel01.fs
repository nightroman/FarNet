module TestPanel01
open FarNet
open FarNet.FSharp

/// Not a panel test but related to panels.
[<Test>]
let testSkipModal = async {
    async {
        // dialog
        Job.StartImmediateFrom showWideDialog
        // wait
        do! Job.SkipModal ()
        // done
        do! job { far.Message "done" }
    }
    |> Job.StartImmediate
    do! job { Assert.True (isWideDialog ()) }

    // exit dialog -> trigger "done" after skipModal
    do! Job.Keys "Esc"
    do! Job.Wait (fun () -> Window.IsDialog () && far.Dialog.[1].Text = "done")

    // exit dialog
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

[<Test>]
let testCannotOpenOnModal = async {
    // dialog
    Job.StartImmediateFrom showWideDialog
    do! job { Assert.Dialog () }

    // try open panel from dialog -> error dialog
    Job.StartImmediate <| Job.OpenPanel (MyPanel.panel [])
    do! Job.Wait (fun () -> Window.IsDialog () && far.Dialog.[1].Text = "Cannot switch to panels.")

    // exit two dialogs
    do! Job.Keys "Esc Esc"
    do! job { Assert.NativePanel () }
}

[<Test>]
let testCanOpenFromEditor = async {
    // editor
    do! job {
        let editor = far.CreateEditor (FileName = far.TempName ())
        editor.DisableHistory <- true
        editor.Open ()
    }
    do! job { Assert.Editor () }

    // panel
    do! Job.OpenPanel (MyPanel.panel [])
    do! job { Assert.True (isMyPanel ()) }

    // exit panel
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }

    // exit editor
    do! Job.Keys "F12 2 Esc"
    do! job {
        Assert.NativePanel ()
        Assert.Equal (2, far.Window.Count)
    }
}
