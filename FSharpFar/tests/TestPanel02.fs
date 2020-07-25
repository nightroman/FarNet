/// Demo/test Job functions for panels.
module TestPanel02
open FarNet
open FarNet.FSharp

/// Opens a panel with 3 items. After closing shows a message with selected items.
/// fs: FarNet.FSharp.Job.Start TestPanel02.flowWaitPanelClosing
let flowWaitPanelClosing = async {
    // open the panel with 3 items
    let! panel = Job.OpenPanel (fun () ->
        PSFar.Invoke "11..13 | Out-FarPanel" |> ignore
    )
    // wait for closing with the function returning selected files
    let! r = Job.WaitPanelClosing (panel, fun _ ->
        panel.SelectedFiles
    )
    // show the returned files
    do! job { far.Message (sprintf "%A" r) }
}

[<Test>]
let testWaitPanelClosing = async {
    Job.Start flowWaitPanelClosing
    do! Job.Wait Window.IsModulePanel
    do! Job.Keys "Down Down Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("seq [12]", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// Opens a panel with 3 items. After closing shows a message "OK".
/// fs: FarNet.FSharp.Job.Start TestPanel02.flowWaitPanelClosed
let flowWaitPanelClosed = async {
    // open the panel with 3 items
    let! panel = Job.OpenPanel (fun () ->
        PSFar.Invoke "1..3 | Out-FarPanel" |> ignore
    )
    // wait for closing
    do! Job.WaitPanelClosed panel
    // show OK
    do! job { far.Message "OK" }
}

[<Test>]
let testWaitPanelClosed = async {
    Job.Start flowWaitPanelClosed
    do! Job.Wait Window.IsModulePanel
    do! Job.Keys "Esc"
    do! job {
        Assert.Dialog ()
        Assert.Equal ("OK", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

/// Fails to open a panel, for testing.
/// fs: FarNet.FSharp.Job.Start TestPanel02.flowOpenPanelFails
let flowOpenPanelFails = async {
    // call OpenPanel with a function not opening a panel
    do! Job.OpenPanel ignore |> Async.Ignore
}

[<Test>]
let testOpenPanelFails = async {
    Job.Start flowOpenPanelFails
    do! Job.Wait Window.IsDialog
    do! job {
        Assert.Equal ("InvalidOperationException", far.Dialog.[0].Text)
        Assert.Equal ("OpenPanel did not open a module panel.", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}
