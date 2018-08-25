
module TestPanel
open FarNet
open FarNet.FSharp
open Test

/// Not a panel test but related to panels.
let testSkipModal = async {
    async {
        // dialog
        Job.Start <| Job.As showWideDialog
        // wait
        do! Job.SkipModal ()
        // done
        do! Job.As (fun () -> far.Message "done")
    }
    |> Job.Start
    do! test isWideDialog

    // exit dialog -> trigger "done" after skipModal
    do! Job.Keys "Esc"
    do! wait (fun () -> isDialog () && dt 1 = "done")

    // exit dialog
    do! Job.Keys "Esc"
    do! test isFarPanel
}

let testCannotOpenOnModal = async {
    // dialog
    Job.Start <| Job.As showWideDialog
    do! test isDialog

    // try open panel from dialog -> error dialog
    Job.Start <| Job.OpenPanel (MyPanel.panel [])
    do! wait (fun () -> isDialog () && dt 1 = "Cannot switch to panels.")

    // exit two dialogs
    do! Job.Keys "Esc Esc"
    do! test isFarPanel
}

let testCanOpenFromEditor = async {
    // editor
    do! Job.As (fun () ->
        let editor = far.CreateEditor (FileName = far.TempName ())
        editor.DisableHistory <- true
        editor.Open ()
    )
    do! test isEditor

    // panel
    do! Job.OpenPanel (MyPanel.panel [])
    do! test isMyPanel

    // exit panel
    do! Job.Keys "Esc"
    do! test isFarPanel

    // exit editor
    do! Job.Keys "F12 2 Esc"
    do! test (fun () -> isFarPanel () && far.Window.Count = 2)
}

let test = async {
    do! testSkipModal
    do! testCannotOpenOnModal
    do! testCanOpenFromEditor
}
