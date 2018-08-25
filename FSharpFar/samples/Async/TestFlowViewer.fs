
module TestFlowViewer
open FarNet
open FarNet.FSharp
open Test

let flowNormal = async {
    let viewer = far.CreateViewer (FileName = __SOURCE_FILE__)
    viewer.DisableHistory <- true
    do! Job.FlowViewer viewer
}
let testNormal = async {
    Job.Start flowNormal

    do! test isViewer
    do! Job.Keys "Esc"

    do! test isFarPanel
}

let flowModal = async {
    // dialog
    Job.As showWideDialog
    |> Job.Start

    // viewer over the dialog
    let viewer = far.CreateViewer (FileName = __SOURCE_FILE__)
    viewer.DisableHistory <- true
    do! Job.FlowViewer viewer

    // OK when viewer closed
    do! Job.As (fun () -> far.Message "OK")
}
let testModal = async {
    Job.Start flowModal

    do! test isViewer
    do! Job.Keys "Esc"

    do! test (isDialogText 1 "OK")
    do! Job.Keys "Esc"

    do! test isWideDialog
    do! Job.Keys "Esc"

    do! test isFarPanel
}

let test = async {
    do! testNormal
    do! testModal
}
