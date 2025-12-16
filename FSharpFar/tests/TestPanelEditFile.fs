module TestPanelEditFile
open FarNet
open FarNet.FSharp
open Swensen.Unquote

// Common test code
let testErrorsOnSaving keys1 (delay: int) keys2 = async {
    // open panel
    do! Jobs.Job PanelEditFile.run

    // edit file
    do! Jobs.Keys "F4"
    do! job {
        test <@ Window.IsEditor() @>
        test <@ "Euler's number (e)" = far.Editor.Title @>
        test <@ "2.71828" = far.Editor.GetText() @>
        test <@ 0 = far.Editor.Line.Caret @> // used to be not 0 sometimes, _201223_vc
    }

    // add some letter, make invalid number, save
    do! Jobs.Keys "End z"
    do! Jobs.Keys keys1

    // assert error dialog
    do! job {
        test <@ Window.IsDialog() @>
        test <@ "Cannot set text" = far.Dialog[0].Text @>
    }

    // exit dialog, wait for the specified time
    do! Jobs.Keys "Esc"
    do! Async.Sleep delay
    do! job {
        test <@ Window.IsEditor() @>
        test <@ 8 = far.Editor.Line.Caret @>
    }

    // exit editor, same Description
    do! Jobs.Keys "Esc"
    do! job {
        test <@ Window.IsModulePanel() @>
        test <@ "2.71828" = far.Panel.CurrentFile.Description @>
    }

    // edit again, valid number
    do! Jobs.Keys "F4"
    do! Jobs.Keys "CtrlA 2 . 7 1"
    do! Jobs.Keys keys2

    // assert updated panel
    do! job {
        test <@ Window.IsModulePanel() @>
        test <@ "2.71" = far.Panel.CurrentFile.Description @>
    }

    // exit panel
    do! Jobs.Keys "Esc"
}

// Errors on saving without closing the editor
Test.Add("testErrorsOnSaving1", testErrorsOnSaving "F2" 1111 "F2 Esc")

// Errors on saving on closing the editor
Test.Add("testErrorsOnSaving2", testErrorsOnSaving "Esc Enter" 0 "Esc Enter")
