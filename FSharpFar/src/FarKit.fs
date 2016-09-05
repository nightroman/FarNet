
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarKit

open FarNet
open System
open System.IO

let far = Far.Api

type UsePanelDirectory() =
    let cd =
        if far.Panel.Kind = PanelKind.File then
            let cd = Environment.CurrentDirectory
            try
                Environment.CurrentDirectory <- far.Panel.CurrentDirectory
                cd
            with _ ->
                null
        else null
    interface IDisposable with
        member x.Dispose() =
            if cd <> null then
                try
                    Environment.CurrentDirectory <- cd
                with _ ->
                    ()

type UseUserScreen() =
    do far.UI.ShowUserScreen()
    interface IDisposable with member x.Dispose() = far.UI.SaveUserScreen()

let fsfLocalData() = far.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.LocalData, true)
let fsfRoaminData() = far.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.RoamingData, true)

let writeException exn =
    use us = new UseUserScreen()
    far.UI.WriteLine(sprintf "%A" exn, ConsoleColor.Red)

let completeLine (editLine : ILine) replacementIndex replacementLength (words : seq<string>) =
    let count = Seq.length words
    let text = editLine.Text

    let word =
        if count = 1 then
             Seq.head words
        else
            let menu = far.CreateListMenu()
            let cursor = far.UI.WindowCursor
            menu.X <- cursor.X
            menu.Y <- cursor.Y
            if count = 0 then
                menu.Add("Empty").Disabled <- true
                menu.NoInfo <- true
                menu.Show() |> ignore
                null
            else
                menu.Incremental <- "*"
                menu.IncrementalOptions <- PatternOptions.Substring
                for word in words do
                    menu.Add(word) |> ignore
                if menu.Show() then
                    menu.Items.[menu.Selected].Text
                else
                    null

    if word <> null then
        let head = text.Substring(0, replacementIndex)
        let caret = head.Length + word.Length
        editLine.Text <- head + word + text.Substring(replacementIndex + replacementLength)
        editLine.Caret <- caret

let showTempFile file title =
    let editor = far.CreateEditor()
    editor.Title <- title
    editor.FileName <- file
    editor.CodePage <- 65001
    editor.IsLocked <- true
    editor.DisableHistory <- true
    editor.DeleteSource <- DeleteSource.UnusedFile
    editor.Open()

let showTempText text title =
    let file = far.TempName("F#") + ".txt"
    File.WriteAllText(file, text)
    showTempFile file title

let isScriptFileName (fileName:string) =
    fileName.EndsWith(".fsx", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".fsscript", StringComparison.OrdinalIgnoreCase)

let completeCode (editor:IEditor) getCompletions =
    let line = editor.Line
    let caret = line.Caret
    if caret = 0 || caret > line.Length then false else

    let text = line.Text
    let completer = Completer.Completer(getCompletions)
    let ok, start, completions = completer.GetCompletions(text, caret)
    if ok then
        completeLine line start (caret - start) completions
        editor.Redraw()
    ok
