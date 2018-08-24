
// Tests the main demo flow and other test scenarios.
// Testing is done by flows concurrent with samples.

open FarNet
open Async
open Test
open App

/// Test the sample wizard flow.
let testWizard = async {
    startJob flowWizard
    do! test isWizard

    // open editor
    do! Job.keys "E"
    do! test isEditor

    // go to panels
    do! Job.keys "F12 1"
    do! test isFarPanel

    // go to editor
    do! Job.keys "F12 2"
    do! test isEditor

    // exit editor
    do! Job.keys "Esc"
    do! test isWizard

    // open my panel
    do! Job.keys "P"
    do! test isMyPanel

    // go to another
    do! Job.keys "Tab"
    do! test isFarPanel

    // go back to mine
    do! Job.keys "Tab"
    do! test isMyPanel

    // exit panel
    do! Job.keys "Esc"
    do! test isWizard

    // OK
    do! Job.keys "Enter"
    do! test isDone

    // done
    do! Job.keys "Esc"
    do! test isFarPanel
}

/// This flow starts the sample flow several times with concurrent testing
/// flows with different test scenarios. Then it starts other test flows.
async {
    // Far windows must be closed
    do! Job.func (fun () -> if far.Window.Count <> 2 then failwith "Close all windows.")

    // test
    do! testWizard
    do! TestError.test
    do! TestFlow01.test
    do! TestFlowDialog.test
    do! TestFlowViewer.test
    do! TestModalCases.test
    do! TestModalEditorIssue.test

    // done
    do! Job.func (fun () -> far.UI.WriteLine __SOURCE_FILE__)
}
|> startJob
