module TestPanelEditFile
open FarNet
open FarNet.FSharp

/// Common test code
let testErrorsOnSaving keys1 delay keys2 = async {
    // open panel
    do! Job.From PanelEditFile.run

    // edit file
    do! Job.Keys "F4"
    do! job {
        Assert.True (Window.IsEditor())
        Assert.Equal("Euler's number (e)", far.Editor.Title)
        Assert.Equal("2.71828", far.Editor.GetText())
        Assert.Equal(0, far.Editor.Line.Caret)
    }

    // add some letter, make invalid number, save
    do! Job.Keys "End z"
    do! Job.Keys keys1

    // assert error dialog
    do! job {
        Assert.True (Window.IsDialog())
        Assert.Equal("Cannot set text", far.Dialog.[0].Text)
    }

    // exit dialog, wait for the specified time
    do! Job.Keys "Esc"
    do! Async.Sleep delay
    do! job {
        Assert.True (Window.IsEditor())
        Assert.Equal(8, far.Editor.Line.Caret)
    }

    // exit editor, same Description
    do! Job.Keys "Esc"
    do! job {
        Assert.True (Window.IsModulePanel())
        Assert.Equal("2.71828", far.Panel.CurrentFile.Description)
    }

    // edit again, valid number
    do! Job.Keys "F4"
    do! Job.Keys "CtrlA 2 . 7 1"
    do! Job.Keys keys2

    // assert updated panel
    do! job {
        Assert.True (Window.IsModulePanel())
        Assert.Equal("2.71", far.Panel.CurrentFile.Description)
    }

    // exit panel
    do! Job.Keys "Esc"
}

/// Errors on saving without closing the editor
[<Test>]
let test1 =
    testErrorsOnSaving "F2" 1111 "F2 Esc"

/// Errors on saving on closing the editor
[<Test>]
let test2 =
    testErrorsOnSaving "Esc Enter" 0 "Esc Enter"
