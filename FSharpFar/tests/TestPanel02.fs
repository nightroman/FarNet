// Demo Jobs functions for panels.
module TestPanel02
open FarNet
open FarNet.FSharp
open Swensen.Unquote

// Opens a panel with 3 items. After closing shows a message with selected items.
let workWaitPanelClosing = async {
    // open the panel with 3 items
    let! panel = Jobs.OpenPanel(fun () ->
        PSFar.Invoke "11..13 | Out-FarPanel" |> ignore
    )
    // wait for closing with the function returning selected files
    let! r = Jobs.WaitPanelClosing(panel, fun _ ->
        panel.GetSelectedFiles()
    )
    // show the returned files
    do! job { far.Message(sprintf "%A" r) }
}

Test.Add("testWaitPanelClosing", async {
    Jobs.Start workWaitPanelClosing
    do! Assert.Wait Window.IsModulePanel
    do! Jobs.Keys "Down Down Esc"
    do! job {
        Assert.Dialog()
        test <@ "[|12|]" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

// Opens a panel with 3 items. After closing shows a message "OK".
let workWaitPanelClosed = async {
    // open the panel with 3 items
    let! panel = Jobs.OpenPanel(fun () ->
        PSFar.Invoke "1..3 | Out-FarPanel" |> ignore
    )
    // wait for closing
    do! Jobs.WaitPanelClosed panel
    // show OK
    do! job { far.Message "OK" }
}

Test.Add("testWaitPanelClosed", async {
    Jobs.Start workWaitPanelClosed
    do! Assert.Wait Window.IsModulePanel
    do! Jobs.Keys "Esc"
    do! job {
        Assert.Dialog()
        test <@ "OK" = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})

// Fails to open a panel, for testing.
let workOpenPanelFails = async {
    // call OpenPanel with a function not opening a panel
    do! Jobs.OpenPanel ignore |> Async.Ignore
}

Test.Add("testOpenPanelFails", async {
    Jobs.Start workOpenPanelFails
    do! Assert.Wait Window.IsDialog
    do! job {
        test <@ "InvalidOperationException" = far.Dialog[0].Text @>
        test <@ "Panel was not opened." = far.Dialog[1].Text @>
    }
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
})
