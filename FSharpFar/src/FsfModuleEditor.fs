
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

//TODO https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information

namespace FSharpFar

open FarNet
open System

[<System.Runtime.InteropServices.Guid("B7916B53-2C17-4086-8F13-5FFCF0D82900")>]
[<ModuleEditor(Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FsfModuleEditor() =
    inherit ModuleEditor()

    let mutable editor:IEditor = null

    // https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
    // EditorTests.fs(265) they use [], "" instead of names, so do we
    let complete() =
        use progress = new UseProgress("Checking...")

        let caret = editor.Caret
        let lineText = editor.[caret.Y].Text

        // end of name
        let nameEnd = caret.X
        if nameEnd = 0 || nameEnd > lineText.Length || Char.IsWhiteSpace(lineText.[nameEnd - 1]) then false else

        // start of name
        let mutable nameStart = nameEnd
        while nameStart > 0 && isIdentChar lineText.[nameStart - 1] do
            nameStart <- nameStart - 1
        let nameToReplace = lineText.Substring(nameStart, nameEnd - nameStart)

        let colAtEndOfPartialName =
            if nameStart > 0 && lineText.[nameStart - 1] = '.' then
                nameStart // = index of dot + 1
            else
                nameEnd // = end + 1

        let config = editor.fsConfig()
        let file = editor.FileName
        let text = editor.GetText()

        let parseResults, checkResults = Checker.check file text config
        let errors = checkResults.Errors

        let decs = checkResults.GetDeclarationListInfo(Some parseResults, caret.Y + 1, colAtEndOfPartialName, lineText, [], "", always false) |> Async.RunSynchronously

        let completions =
            decs.Items
            |> Seq.map (fun item -> item.Name)
            |> Seq.filter (fun name -> name.StartsWith nameToReplace)

        progress.Done()

        completeLine editor.Line nameStart nameToReplace.Length completions
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
