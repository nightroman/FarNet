
// Starts the flow several times for automatic testing.
// Testing is done by flows concurrent with the sample.

open FarNet
open Async
open App
open System
open System.Diagnostics

/// Check delay. It is not for waiting results! It should work with 0, too, but
/// it flows too fast, we cannot see anything. With some not too small value we
/// can see the flow in progress.
let delay = 100
let wait predicate = async {
    Debug.WriteLine (sprintf "!! wait %A" predicate)
    let! ok = Job.await delay 500 5000 predicate
    if not ok then failwithf "Timeout %A" predicate
}

let dt index =
    if far.Window.Kind <> WindowKind.Dialog then failwith "Expected dialog."
    far.Dialog.[index].Text

let isDialog () =
    far.Window.Kind = WindowKind.Dialog

let isEditor () =
    far.Window.Kind = WindowKind.Editor && far.Editor.Title = "Demo title"

let isViewer () =
    far.Window.Kind = WindowKind.Viewer

let isWizard () =
    isDialog () && dt 0 = "Wizard"

let isDone () =
    isDialog () && dt 0 = "Done"

let isError () =
    isDialog () && dt 0 = "Exception" && dt 1 = "Oh"

let isMyPanel () =
    far.Window.Kind = WindowKind.Panels && far.Panel.IsPlugin && (
        let p = far.Panel :?> Panel
        p.Title = "MyPanel"
    )

let isFarPanel () =
    far.Window.Kind = WindowKind.Panels && not far.Panel.IsPlugin

/// Test the sample wizard flow.
let testWizard = async {
    Async.farStart flowWizard
    do! wait isWizard

    // open editor
    do! Job.keys "E"
    do! wait isEditor

    // go to panels
    do! Job.keys "F12 1"
    do! wait isFarPanel

    // go to editor
    do! Job.keys "F12 2"
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isWizard

    // open my panel
    do! Job.keys "P"
    do! wait isMyPanel

    // go to another
    do! Job.keys "Tab"
    do! wait isFarPanel

    // go back to mine
    do! Job.keys "Tab"
    do! wait isMyPanel

    // exit panel
    do! Job.keys "Esc"
    do! wait isWizard

    // OK
    do! Job.keys "Enter"
    do! wait isDone

    // done
    do! Job.keys "Esc"
    do! wait isFarPanel
}

/// Similar to flowWizard but linear and with cancel and error cases.
let flowTest = async {
    let mutable text = "Hello,\nWorld!"
    let mutable answer = 1

    // do non-modal editor loop with options for the next step
    while answer = 1 do
        // edit some text in the editor
        let! r = jobEditText text "Demo title"
        text <- r

        // ask how to continue
        let! r = jobAsk text "Wizard" [|"&Yes"; "&No"; "&Cancel"; "&Error"|]
        match r with
        | 2 -> do! Job.cancel
        | 3 -> failwith "Oh"
        | _ -> answer <- r

    // open panel and wait for closing
    let lines = text.Split [|'\n'|] |> Seq.cast
    do! Job.flowPanel (MyPanel.panel lines)

    // show final message
    do! Job.func (fun () -> far.Message (text, "Done"))
}

/// The full flow with one return to the editor.
let testMainWithNo = async {
    // start and wait for editor
    Async.farStart flowTest
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isWizard

    // No -> repeat editor
    do! Job.keys "N"
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isWizard

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
    Async.farStart flowTest
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isWizard

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
    Async.farStart flowTest
    do! wait isEditor

    // exit editor
    do! Job.keys "Esc"
    do! wait isWizard

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
    do! wait (fun () -> dt 0 = "job4.2")
    do! Job.keys "Esc"

    // test and exit dialog 1
    do! wait (fun () -> dt 0 = "job4.1")
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
    do! wait (fun () -> dt 1 = name)

    // test and exit dialog
    do! wait (fun () -> dt 1 = name)
    do! Job.keys "Esc"

    // done
    do! wait isFarPanel
}

module ModalDialogEditorIssues =
    let flow = async {
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
    let test = async {
        Async.farStart flow

        // nasty Far message
        do! wait (fun () -> isDialog () && dt 1 = "It is impossible to edit the folder")
        do! Job.keys "Esc"

        // posted FarNet error
        do! wait (fun () -> dt 0 = "InvalidOperationException")
        do! Job.keys "Esc"

        // posted FarNet error
        do! wait (fun () -> dt 0 = "before editor")
        do! Job.keys "Esc"

        // done
        do! wait isFarPanel
    }

module ModalWithError =
    let flow = async {
        // modal with exception
        do! Job.modal (fun () ->
            failwith "in-modal"
        )
        failwith "unexpected"
    }
    let test = async {
        Async.farStart flow
        do! wait (fun () -> isDialog () && dt 0 = "Exception" && dt 1 = "in-modal")
        do! Job.keys "Esc"
        do! wait isFarPanel
    }

module MacroInvalid =
    let flow = async {
        // invalid macro
        do! Job.macro "bar"
        // not called
        failwith "unexpected"
    }
    let test = async {
        Async.farStart flow
        // our async exception
        do! wait (fun () -> isDialog () && dt 0 = "ArgumentException" && dt 3 = "Macro: bar" && dt 4 = "Parameter name: macro")
        do! Job.keys "Esc"
        // done
        do! wait isFarPanel
    }

module FlowViewer =
    let flowNormal = async {
        let viewer = far.CreateViewer (FileName = __SOURCE_DIRECTORY__)
        do! Job.flowViewer viewer
    }
    let flowModal = async {
        do! Job.modal (fun () -> far.Message "long_text_message_for_wide_dialog")

        let viewer = far.CreateViewer (FileName = __SOURCE_DIRECTORY__)
        do! Job.flowViewer viewer

        do! Job.func (fun () -> far.Message "OK")
    }
    let test = async {
        Async.farStart flowNormal
        do! wait (fun () -> isViewer ())
        do! Job.keys "Esc"
        do! wait isFarPanel

        Async.farStart flowModal
        do! wait (fun () -> isViewer ())
        do! Job.keys "Esc"
        do! wait (fun () -> isDialog () && dt 1 = "OK")
        do! Job.keys "Esc"
        do! wait (fun () -> isDialog () && dt 1 = "long_text_message_for_wide_dialog")
        do! Job.keys "Esc"
        do! wait isFarPanel
    }

/// This flow starts the sample flow several times with concurrent testing
/// flows with different test scenarios. Then it starts other test flows.
async {
    do! Job.func (fun () -> if far.Window.Count <> 2 then failwith "Close all but panels.")

    // wizard sample
    do! testWizard

    // similar sample
    do! testMainWithNo
    do! testMainWithError
    do! testMainWithCancel

    // modal
    do! testModalDialogDialog
    do! testModalDialogEditor
    do! ModalDialogEditorIssues.test
    do! ModalWithError.test

    // macro
    do! MacroInvalid.test

    // viewer
    do! FlowViewer.test

    // done
    far.UI.WriteLine (DateTime.Now.ToString ())
}
|> Async.farStart
