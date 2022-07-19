module TestWizard
open Wizard
open FarNet
open FarNet.FSharp

// Test the demo wizard job.
Test.Add("testWizard", async {
    Jobs.StartImmediate jobWizard
    do! job { Assert.True(isWizard ()) }

    // open editor
    do! Jobs.Keys "E"
    do! Assert.Wait Window.IsEditor

    // go to panels
    do! Jobs.Keys "F12 1"
    do! job { Assert.NativePanel() }

    // go to editor
    do! Jobs.Keys "F12 2"
    do! job { Assert.Editor() }

    // exit editor
    do! Jobs.Keys "Esc"
    do! job { Assert.True(isWizard ()) }

    // open my panel
    do! Jobs.Keys "P"
    do! Assert.Wait isMyPanel

    // go to another
    do! Jobs.Keys "Tab"
    do! job { Assert.NativePanel() }

    // go back to mine
    do! Jobs.Keys "Tab"
    do! job { Assert.True(isMyPanel ()) }

    // exit panel
    do! Jobs.Keys "Esc"
    do! job { Assert.True(isWizard ()) }

    // OK
    do! Jobs.Keys "Enter"
    do! job {
        Assert.Dialog()
        Assert.Equal("Done", far.Dialog[0].Text)
    }

    // done
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})
