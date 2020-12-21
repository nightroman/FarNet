module TestWizard
open Wizard
open FarNet
open FarNet.FSharp

/// Test the sample wizard flow.
[<Test>]
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
    do! waitSteps ()
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
