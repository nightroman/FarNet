
(*
    Starts the flow several times for automatic testing.
    Testing is done by flows concurrent with the sample.
*)

open FarNet
open Async

let wait predicate = async {
    let! ok = Job.await 500 500 5000 predicate
    if not ok then
        invalidOp "Timeout!"
}

let isEditor () =
    far.Window.Kind = WindowKind.Editor && far.Editor.Title = "Demo title"

let isContinue () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Continue"

let isDone () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "Done"

let isError () =
    far.Window.Kind = WindowKind.Dialog && far.Dialog.[0].Text = "InvalidOperationException" && far.Dialog.[1].Text = "Oh"

let isMyPanel () =
    far.Window.Kind = WindowKind.Panels && far.Panel.IsPlugin && (
        let p = far.Panel :?> Panel
        p.Title = "MyPanel"
    )

let isFarPanel () =
    far.Window.Kind = WindowKind.Panels && not far.Panel.IsPlugin

/// The full flow with one return to the editor.
let job1 = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    Job.postMacro "Keys'Esc'"
    do! wait isContinue

    // No -> repeat editor
    Job.postMacro "Keys'N'"
    do! wait isEditor

    // exit editor
    Job.postMacro "Keys'Esc'"
    do! wait isContinue

    // Yes -> my panel
    Job.postMacro "Keys'Y'"
    do! wait isMyPanel

    // exit panel -> dialog
    Job.postMacro "Keys'Esc'"
    do! wait isDone

    // exit dialog
    Job.postMacro "Keys'Esc'"
    do! wait isFarPanel
}

/// The flow is stopped by an exception.
let job2 = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    Job.postMacro "Keys'Esc'"
    do! wait isContinue

    // Error -> dialog
    Job.postMacro "Keys'E'"
    do! wait isError

    // exit dialog
    Job.postMacro "Keys'Esc'"
    do! wait isFarPanel
}

/// The flow is stopped by cancelling.
let job3 = async {
    // start and wait for editor
    Async.farStart App.flow
    do! wait isEditor

    // exit editor
    Job.postMacro "Keys'Esc'"
    do! wait isContinue

    // Cancel -> panels
    Job.postMacro "Keys'C'"
    do! wait isFarPanel
}

/// This flow starts the sample flow several times with concurrent testing
/// flows with different test scenarios.
async {
    do! Job.fromFunc (fun () -> if far.Window.Count <> 2 then invalidOp "Close all but panels.")
    do! job1
    do! job2
    do! job3
}
|> Async.farStart
