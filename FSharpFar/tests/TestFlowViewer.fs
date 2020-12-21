module TestFlowViewer
open FarNet
open FarNet.FSharp

let fileName = __SOURCE_DIRECTORY__ + "\\" + __SOURCE_FILE__

let flowNormal = async {
    let viewer = far.CreateViewer (FileName = fileName)
    viewer.DisableHistory <- true
    do! Job.FlowViewer viewer
}

[<Test>]
let testNormal = async {
    Job.StartImmediate flowNormal
    do! job { Assert.Viewer () }
    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}

let flowModal = async {
    // dialog
    Job.StartImmediate(Job.From showWideDialog)

    // viewer over the dialog
    let viewer = far.CreateViewer (FileName = fileName)
    viewer.DisableHistory <- true
    do! Job.FlowViewer viewer

    // OK when viewer closed
    do! job { far.Message "OK" }
}

[<Test>]
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
