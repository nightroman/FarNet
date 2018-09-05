
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
    Job.StartImmediate flowNormal

    do! test isViewer
    do! Job.Keys "Esc"

    do! test isFarPanel
}

let flowModal = async {
    // dialog
    Job.StartImmediateFrom showWideDialog

    // viewer over the dialog
    let viewer = far.CreateViewer (FileName = __SOURCE_FILE__)
    viewer.DisableHistory <- true
    do! Job.FlowViewer viewer

    // OK when viewer closed
    do! job { far.Message "OK" }
}
let testModal = async {
    Job.StartImmediate flowModal

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
