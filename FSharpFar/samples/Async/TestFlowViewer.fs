
module TestFlowViewer
open FarNet
open Async
open Test

let flowNormal = async {
    let viewer = far.CreateViewer (FileName = __SOURCE_FILE__, DisableHistory = true)
    do! Job.flowViewer viewer
}
let testNormal = async {
    startJob flowNormal

    do! test isViewer
    do! Job.keys "Esc"

    do! test isFarPanel
}

let flowModal = async {
    // dialog
    Job.func showWideDialog
    |> startJob

    // viewer over the dialog
    let viewer = far.CreateViewer (FileName = __SOURCE_FILE__, DisableHistory = true)
    do! Job.flowViewer viewer

    // OK when viewer closed
    do! Job.func (fun () -> far.Message "OK")
}
let testModal = async {
    startJob flowModal

    do! test isViewer
    do! Job.keys "Esc"

    do! test (isDialogText 1 "OK")
    do! Job.keys "Esc"

    do! test isWideDialog
    do! Job.keys "Esc"

    do! test isFarPanel
}

let test = async {
    do! testNormal
    do! testModal
}
