
module TestPanel02
open Test
open FarNet
open FarNet.FSharp

let flowOpenPanelFails = async {
    do! Job.OpenPanel ignore |> Async.Ignore
}
let testOpenPanelFails = async {
    Job.Start flowOpenPanelFails
    do! wait isDialog
    do! test (isDialogText 0 "InvalidOperationException")
    do! test (isDialogText 1 "OpenPanel did not open a module panel.")
    do! Job.Keys "Esc"
    do! test isFarPanel
}

let flowWaitPanelClosed = async {
    let! panel = Job.OpenPanel (fun () ->
        PowerShellFar.invokeScript "1..3 | Out-FarPanel" null |> ignore
    )
    do! Job.WaitPanelClosed panel
    do! job { far.Message "OK" }
}
let testWaitPanelClosed = async {
    Job.Start flowWaitPanelClosed
    do! wait isModulePanel
    do! Job.Keys "Esc"
    do! test (isDialogText 1 "OK")
    do! Job.Keys "Esc"
    do! test isFarPanel
}

let flowWaitPanelClosing = async {
    let! panel = Job.OpenPanel (fun () ->
        PowerShellFar.invokeScript "11..13 | Out-FarPanel" null |> ignore
    )
    let! r = Job.WaitPanelClosing (panel, fun _ ->
        panel.SelectedFiles
    )
    do! job { far.Message (sprintf "%A" r) }
}
let testWaitPanelClosing = async {
    Job.Start flowWaitPanelClosing
    do! wait isModulePanel
    do! Job.Keys "Down Down Esc"
    do! test (isDialogText 1 "seq [12]")
    do! Job.Keys "Esc"
    do! test isFarPanel
}

let test = async {
    do! testOpenPanelFails
    do! testWaitPanelClosed
    do! testWaitPanelClosing
}
