module TestTasks
open FarNet
open FarNet.FSharp
open System

/// Use Tasks.Job, Tasks.Job<T>, Tasks.Editor, Tasks.Keys
let flowTasks = async {
    // task with action
    do! Tasks.Job(fun() -> far.Message("Action")) |> Async.AwaitTask

    // task with function
    let! r = Tasks.Job(fun() -> far.Input("Function")) |> Async.AwaitTask
    Assert.Equal("bar", r)

    // task with editor
    let editor = far.CreateEditor();
    editor.FileName <- __SOURCE_DIRECTORY__ + "/" + __SOURCE_FILE__;
    editor.DisableHistory <- true
    do! Tasks.Editor(editor) |> Async.AwaitTask

    // task with keys
    do! Tasks.Keys("CtrlG") |> Async.AwaitTask
}

[<Test>]
let testTasks = async {
    Job.StartImmediate flowTasks

    // message box
    do! Job.Wait Window.IsDialog
    Assert.Equal("Action", far.Dialog.[1].Text)
    do! Job.Keys "Esc"

    // input box, enter "bar"
    do! Job.Wait Window.IsDialog
    Assert.Equal("Function", far.Dialog.[1].Text)
    do! Job.Keys "b a r Enter"

    // editor, exit
    do! Job.Wait Window.IsEditor
    Assert.True(far.Editor.Title.EndsWith(__SOURCE_FILE__));
    do! Job.Keys "Esc"

    // CtrlG dialog, exit
    do! Job.Wait Window.IsDialog
    Assert.Equal(Guid("044ef83e-8146-41b2-97f0-404c2f4c7b69"), far.Dialog.TypeId)
    do! Job.Keys "Esc"
}

/// Call EditTextAsync twice, expected text 1: $x=1; 2: $x=3
let flowEditText = async {
    let args = EditTextArgs()
    args.Text <- ""
    args.Extension <- "ps1"

    let! text = far.AnyEditor.EditTextAsync(args) |> Async.AwaitTask
    Assert.Equal("$x=1", text)

    args.Text <- "$x=2"
    let! text = far.AnyEditor.EditTextAsync(args) |> Async.AwaitTask
    Assert.Equal("$x=3", text)
}

[<Test>]
let testEditText = async {
    Job.StartImmediate flowEditText

    // type $x=1, save, exit
    do! Job.Wait (fun() -> Window.IsEditor() && far.Editor.GetText() = "")
    do! Job.Keys "$ x = 1 F2 Esc"

    // select all, type $x=2, save, exit
    do! Job.Wait (fun() -> Window.IsEditor() && far.Editor.[0].Text = "$x=2")
    do! Job.Keys "CtrlA $ x = 3 F2 Esc"
}
