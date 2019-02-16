// Tests the main demo flow and other test scenarios.
// Testing is done by flows concurrent with samples.

open Test
open FarNet
open FarNet.FSharp
open App

/// Test the sample wizard flow.
let testWizard = async {
    Job.StartImmediate flowWizard
    do! job { Assert.True (isWizard ()) }

    // open editor
    do! Job.Keys "E"
    do! job { Assert.Editor () }

    // go to panels
    do! Job.Keys "F12 1"
    do! job { Assert.NativePanel () }

    // go to editor
    do! Job.Keys "F12 2"
    do! job { Assert.Editor () }

    // exit editor
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // open my panel
    do! Job.Keys "P"
    do! job { Assert.True (isMyPanel ()) }

    // go to another
    do! Job.Keys "Tab"
    do! job { Assert.NativePanel () }

    // go back to mine
    do! Job.Keys "Tab"
    do! job { Assert.True (isMyPanel ()) }

    // exit panel
    do! Job.Keys "Esc"
    do! job { Assert.True (isWizard ()) }

    // OK
    do! Job.Keys "Enter"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("Done", far.Dialog.[0].Text)
    }

    // done
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// This flow starts the sample flow several times with concurrent testing
/// flows with different test scenarios. Then it starts other test flows.
async {
    // Far windows must be closed
    do! job { if far.Window.Count <> 2 then failwith "Close all windows." }

    // test
    do! testWizard
    do! Parallel.test
    do! TestError.test
    do! TestFlow01.test
    do! TestFlowDialog.test
    do! TestFlowViewer.test
    do! TestModalCases.test
    do! TestModalEditorIssue.test
    do! TestPanel01.test
    do! TestPanel02.test

    // done
    do! job { far.UI.WriteLine __SOURCE_FILE__ }
}
|> Job.StartImmediate
