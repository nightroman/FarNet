module TestViewer
open FarNet
open FarNet.FSharp

let fileName = __SOURCE_DIRECTORY__ + "\\" + __SOURCE_FILE__

let workNormal = async {
    let viewer = far.CreateViewer(FileName = fileName)
    viewer.DisableHistory <- true
    do! Jobs.Viewer viewer
}

[<Test>]
let testNormal = async {
    Jobs.StartImmediate workNormal
    do! job { Assert.Viewer() }
    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
}

let workModal = async {
    // dialog
    Jobs.StartImmediate(Jobs.Job showWideDialog)

    // viewer over the dialog
    let viewer = far.CreateViewer(FileName = fileName)
    viewer.DisableHistory <- true
    do! Jobs.Viewer viewer

    // OK when viewer closed
    do! job { far.Message "OK" }
}

[<Test>]
let testModal = async {
    Jobs.StartImmediate workModal
    do! job { Assert.Viewer() }
    do! Jobs.Keys "Esc"

    do! job {
        Assert.Dialog()
        Assert.Equal("OK", far.Dialog[1].Text)
    }
    do! Jobs.Keys "Esc"

    do! job { Assert.True(isWideDialog ()) }
    do! Jobs.Keys "Esc"

    do! job { Assert.NativePanel() }
}
