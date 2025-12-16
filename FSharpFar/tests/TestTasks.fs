module TestTasks
open FarNet
open FarNet.FSharp
open System
open Swensen.Unquote

// Use Tasks.Job, Tasks.Job<T>, Tasks.Editor, Tasks.Keys
let workTasks = async {
    // task with action
    do! Tasks.Job(fun () -> far.Message("Action")) |> Async.AwaitTask

    // task with function
    let! r = Tasks.Job(fun () -> far.Input("Function")) |> Async.AwaitTask
    test <@ "bar" = r @>

    // task with editor
    let editor = far.CreateEditor();
    editor.FileName <- __SOURCE_DIRECTORY__ + "/" + __SOURCE_FILE__;
    editor.DisableHistory <- true
    do! Tasks.Editor(editor) |> Async.AwaitTask

    // task with keys
    do! Tasks.Keys("CtrlG") |> Async.AwaitTask
}

Test.Add("testTasks", async {
    Jobs.StartImmediate workTasks

    // message box
    do! Assert.Wait Window.IsDialog
    do! job { test <@ "Action" = far.Dialog[1].Text @> }
    do! Jobs.Keys "Esc"

    // input box, enter "bar"
    do! Assert.Wait Window.IsDialog
    do! job { test <@ "Function" = far.Dialog[1].Text @> }
    do! Jobs.Keys "b a r Enter"

    // editor, exit
    do! Assert.Wait Window.IsEditor
    do! job { test <@ far.Editor.Title.EndsWith(__SOURCE_FILE__) @> }
    do! Jobs.Keys "Esc"

    // CtrlG dialog, exit
    do! Assert.Wait Window.IsDialog
    do! job { test <@ Guid("044ef83e-8146-41b2-97f0-404c2f4c7b69") = far.Dialog.TypeId @> }
    do! Jobs.Keys "Esc"
})

// Call EditTextAsync twice, expected text 1: $x=1; 2: $x=3
let workEditText = async {
    let args = EditTextArgs()
    args.Text <- ""
    args.Extension <- "ps1"

    let! text = far.AnyEditor.EditTextAsync(args) |> Async.AwaitTask
    test <@ "$x=1" = text @>

    args.Text <- "$x=2"
    let! text = far.AnyEditor.EditTextAsync(args) |> Async.AwaitTask
    test <@ "$x=3" = text @>
}

Test.Add("testEditText", async {
    Jobs.StartImmediate workEditText

    // type $x=1, save, exit
    do! Assert.Wait(fun () -> Window.IsEditor() && far.Editor.GetText() = "")
    do! Jobs.Keys "$ x = 1 F2 Esc"

    // select all, type $x=2, save, exit
    do! Assert.Wait(fun () -> Window.IsEditor() && far.Editor[0].Text = "$x=2")
    do! Jobs.Keys "CtrlA $ x = 3 F2 Esc"
})
