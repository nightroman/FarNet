
(*
    Starts the flow several times for automatic testing.
    Testing is done by flows concurrent with the sample.
*)

open FarNet
open Async
open System.Diagnostics

/// Check delay. It is not for waiting results! It should work with 0, too, but
/// it flows too fast, we cannot see anything. With some not too small value we
/// can see the flow in progress.
let delay = 200
let wait predicate = async {
    Debug.WriteLine (sprintf "!! wait %A" predicate)
    let! ok = Job.await delay 500 5000 predicate
    if not ok then failwithf "Timeout %A" predicate
}

let isEditor () =
    far.Window.Kind = WindowKind.Editor && far.Editor.Title = "Demo title"

let isContinue () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Continue"

let isDone () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Done"

let isError () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Exception" && far.Dialog.[1].Text = "Oh"

let isMyPanel () =
    far.Window.Kind = WindowKind.Panels && far.Panel.IsPlugin && (
        let p = far.Panel :?> Panel
        p.Title = "MyPanel"
    )

let isFarPanel () =
    far.Window.Kind = WindowKind.Panels && not far.Panel.IsPlugin

/// The full flow with one return to the editor.
let testMainWithNo = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isContinue

    // No -> repeat editor
    do! Job.keys "N"
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isContinue

    // Yes -> my panel
    do! Job.keys "Y"
    do! wait isMyPanel

    // exit panel -> dialog
    do! Job.keys "Esc"
    do! wait isDone

    // exit dialog
    do! Job.keys "Esc"
    do! wait isFarPanel
}

/// The flow is stopped by an exception.
let testMainWithError = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isContinue

    // Error -> dialog
    do! Job.keys "E"
    do! wait isError

    // exit dialog
    do! Job.keys "Esc"
    do! wait isFarPanel
}

/// The flow is stopped by cancelling.
let testMainWithCancel = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isContinue

    // Cancel -> panels
    do! Job.keys "C"
    do! wait isFarPanel
}

/// Test Job.modal
let testModalDialogDialog = async {
    // dialog 1
    do! Job.modal (fun () ->
        far.Message ("some long text to make a wide dialog", "job4.1")
    )

    // dialog 2 on top of 1
    do! Job.modal (fun () ->
        far.Message ("ok", "job4.2")
    )

    // test and exit dialog 2
    do! wait (fun () -> far.Dialog.[0].Text = "job4.2")
    do! Job.keys "Esc"

    // test and exit dialog 1
    do! wait (fun () -> far.Dialog.[0].Text = "job4.1")
    do! Job.keys "Esc"

    // done
    do! wait isFarPanel
}

/// Test Job.modal
let testModalDialogEditor = async {
    let name = "testModalDialogEditor"

    // dialog
    do! Job.modal (fun () ->
        far.Message name
    )

    // editor
    let editor = far.CreateEditor ()
    editor.Title <- name
    do! Job.modal editor.Open

    // test and exit editor
    do! wait (fun () -> far.Editor.Title = name)
    do! Job.keys "Esc"
    do! wait (fun () -> far.Dialog.[1].Text = name)

    // test and exit dialog
    do! wait (fun () -> far.Dialog.[1].Text = name)
    do! Job.keys "Esc"

    // done
    do! wait isFarPanel
}

let jobModalDialogEditorIssues = async {
    // dialog
    do! Job.modal (fun () ->
        far.Message ("".PadLeft (80, '!'), "before editor")
    )

    // editor with problems
    let editor = far.CreateEditor ()
    editor.FileName <- __SOURCE_DIRECTORY__
    do! Job.flowEditor editor

    failwith "unexpected"
}
let testModalDialogEditorIssues = async {
    // start
    Async.farStart jobModalDialogEditorIssues

    // nasty Far message
    do! wait (fun () -> far.Window.Kind = WindowKind.Dialog && far.Dialog.[1].Text = "It is impossible to edit the folder")
    do! Job.keys "Esc"

    // posted FarNet error
    do! wait (fun () -> far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "System.InvalidOperationException")
    do! Job.keys "Esc"

    // posted FarNet error
    do! wait (fun () -> far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "before editor")
    do! Job.keys "Esc"

    // done
    do! wait isFarPanel
}

let jobModalWithError = async {
    // modal with exception
    do! Job.modal (fun () ->
        failwith "in-modal"
    )
    failwith "unexpected"
}
let testModalWithError = async {
    Async.farStart jobModalWithError
    do! wait (fun () -> far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Exception" && far.Dialog.[1].Text = "in-modal")
    do! Job.keys "Esc"
    do! wait isFarPanel
}

/// This flow starts the sample flow several times with concurrent testing
/// flows with different test scenarios. Then it starts other test flows.
async {
    do! Job.func (fun () -> if far.Window.Count <> 2 then failwith "Close all but panels.")

    // sample tests
    do! testMainWithNo
    do! testMainWithError
    do! testMainWithCancel

    // other tests
    do! testModalDialogDialog
    do! testModalDialogEditor
    do! testModalDialogEditorIssues
    do! testModalWithError
}
|> Async.farStart
