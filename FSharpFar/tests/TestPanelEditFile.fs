module TestPanelEditFile
open FarNet
open FarNet.FSharp

/// Common test code
let testErrorsOnSaving keys1 (delay: int) keys2 = async {
    // open panel
    do! Jobs.Job PanelEditFile.run

    // edit file
    do! Jobs.Keys "F4"
    do! job {
        Assert.True (Window.IsEditor())
        Assert.Equal("Euler's number (e)", far.Editor.Title)
        Assert.Equal("2.71828", far.Editor.GetText())
        Assert.Equal(0, far.Editor.Line.Caret) // used to be not 0 sometimes, _201223_vc
    }

    // add some letter, make invalid number, save
    do! Jobs.Keys "End z"
    do! Jobs.Keys keys1

    // assert error dialog
    do! job {
        Assert.True (Window.IsDialog())
        Assert.Equal("Cannot set text", far.Dialog[0].Text)
    }

    // exit dialog, wait for the specified time
    do! Jobs.Keys "Esc"
    do! Async.Sleep delay
    do! job {
        Assert.True (Window.IsEditor())
        Assert.Equal(8, far.Editor.Line.Caret)
    }

    // exit editor, same Description
    do! Jobs.Keys "Esc"
    do! job {
        Assert.True (Window.IsModulePanel())
        Assert.Equal("2.71828", far.Panel.CurrentFile.Description)
    }

    // edit again, valid number
    do! Jobs.Keys "F4"
    do! Jobs.Keys "CtrlA 2 . 7 1"
    do! Jobs.Keys keys2

    // assert updated panel
    do! job {
        Assert.True (Window.IsModulePanel())
        Assert.Equal("2.71", far.Panel.CurrentFile.Description)
    }

    // exit panel
    do! Jobs.Keys "Esc"
}

/// Errors on saving without closing the editor
[<Test>]
let test1 =
    testErrorsOnSaving "F2" 1111 "F2 Esc"

/// Errors on saving on closing the editor
[<Test>]
let test2 =
    testErrorsOnSaving "Esc Enter" 0 "Esc Enter"
