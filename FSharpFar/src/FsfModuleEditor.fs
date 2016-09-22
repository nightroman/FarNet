
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

//TODO https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information

namespace FSharpFar

open FarNet
open System
open Checker
open FsAutoComplete

[<System.Runtime.InteropServices.Guid("B7916B53-2C17-4086-8F13-5FFCF0D82900")>]
[<ModuleEditor(Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FsfModuleEditor() =
    inherit ModuleEditor()

    let mutable editor:IEditor = null

    // https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
    // old EditorTests.fs(265) they use [], "" instead of names, so do we.
    // new Use FsAutoComplete way.
    let complete() =
        use progress = new UseProgress("Checking...")

        let caret = editor.Caret
        let line = editor.[caret.Y]
        if caret.X = 0 || caret.X > line.Length then false
        else

        let lineStr = line.Text
        if Char.IsWhiteSpace lineStr.[caret.X - 1] then false
        else

        let options = getOptionsForFile editor.FileName editor.fsSession
        let file = editor.FileName
        let text = editor.GetText()

        let parseResults, checkResults = Checker.check file text options

        let longName, residue = Parsing.findLongIdentsAndResidue(caret.X, lineStr)
        let decs = checkResults.GetDeclarationListInfo(Some parseResults, caret.Y + 1, caret.X + 1, lineStr, longName, residue, always false) |> Async.RunSynchronously

        let completions =
            decs.Items
            |> Seq.map (fun item -> item.Name)
            |> Seq.filter (fun name -> name.StartsWith residue)

        progress.Done()

        completeLine editor.Line (caret.X - residue.Length) residue.Length completions
        editor.Redraw()
        true

    override x.Invoke(sender, e) =
        editor <- sender
        if editor.fsSession.IsNone then
            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is() && not editor.SelectionExists ->
                     e.Ignore <- complete()
                | _ -> ()
