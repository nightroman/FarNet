
/// Common test tools.
module Test
open FarNet
open FarNet.FSharp

/// Check delay. It is not for waiting results! It should work with 0, too, but
/// it plays fast and we cannot see much. With larger value we can see the play.
let mutable delay = 0

/// Checks for the predicate and throws if it is false.
/// Use it if the result readiness is clear, else use `wait`.
let test predicate = async {
    do! Async.Sleep delay
    do! job {
        if not (predicate ()) then
            failwithf "False predicate %A" predicate
    }
}

/// Waits for some time until the predicate is true.
/// Use it if the result readiness is not clear for `test`.
let wait predicate = async {
    let! ok = Job.Wait (delay, 200, 5000, predicate)
    if not ok then failwithf "Timeout predicate %A" predicate
}

let dt index =
    if far.Window.Kind <> WindowKind.Dialog then failwith "Expected dialog."
    far.Dialog.[index].Text

/// It returns the predicate created at the calling line. This line is shown in errors.
/// That is why `do! test (isDialogText ...)` is better than `do! testDialogText ...`.
let isDialogText index text =
    fun () ->
        if far.Window.Kind <> WindowKind.Dialog then failwith "Expected dialog."
        far.Dialog.[index].Text = text

let isDialog () =
    far.Window.Kind = WindowKind.Dialog

let isEditor () =
    far.Window.Kind = WindowKind.Editor

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

let showWideDialog () =
    far.Message "relatively_long_text_message_for_relatively_wide_dialog"

let isWideDialog () =
    isDialog () && dt 1 = "relatively_long_text_message_for_relatively_wide_dialog"
