
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
    do! job { Assert.Viewer () }
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
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
    do! job { Assert.Viewer () }
    do! Job.Keys "Esc"

    do! job {
        Assert.Dialog ()
        Assert.Equal ("OK", far.Dialog.[1].Text)
    }
    do! Job.Keys "Esc"

    do! job { Assert.True (isWideDialog ()) }
    do! Job.Keys "Esc"

    do! job { Assert.NativePanel () }
}

let test = async {
    do! testNormal
    do! testModal
}
