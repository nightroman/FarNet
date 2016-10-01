
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

//TODO https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information

namespace FSharpFar

open FarNet
open System
open Checker
open FsAutoComplete

[<System.Runtime.InteropServices.Guid "B7916B53-2C17-4086-8F13-5FFCF0D82900">]
[<ModuleEditor (Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FarEditor () =
    inherit ModuleEditor ()

    let mutable editor:IEditor = null

    // https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
    // old EditorTests.fs(265) they use [], "" instead of names, so do we.
    // new Use FsAutoComplete way.
    let complete () =
        use progress = new Progress "Checking..."

        // skip out of text
        let caret = editor.Caret
        let line = editor.[caret.Y]
        if caret.X = 0 || caret.X > line.Length then false
        else

        // skip no solid base
        //TODO completion of parameters? :: x (y, [Tab]
        let lineStr = line.Text
        if Char.IsWhiteSpace lineStr.[caret.X - 1] then false
        else

        // parse
        let names, residue = Parsing.findLongIdentsAndResidue (caret.X, lineStr)

(*
    _160922_160602
    Complete `x.ToString().C` incorrectly gives all globals.
    But complete `x.ToString().` gives string members.
    Let's reduce to the working fine case.
*)
        let mutable residue2 = residue
        let mutable colAtEndOfPartialName = caret.X + 1
        let isDot () =
            let i = caret.X - 1 - residue.Length
            i > 0 && lineStr.[i] = '.'
        if residue.Length > 0 && names.IsEmpty && isDot () then
            residue2 <- ""
            colAtEndOfPartialName <- colAtEndOfPartialName - residue.Length

        let options = editor.getOptions ()
        let file = editor.FileName
        let text = editor.GetText ()

        let res = Checker.check file text options

        let decs = res.CheckResults.GetDeclarationListInfo (Some res.ParseResults, caret.Y + 1, colAtEndOfPartialName, lineStr, names, residue2, always false) |> Async.RunSynchronously

        let completions =
            decs.Items
            |> Seq.map (fun item -> item.Name)
            |> Seq.filter (fun name -> name.StartsWith residue)

        progress.Done ()

        completeLine editor.Line (caret.X - residue.Length) residue.Length completions
        editor.Redraw ()
        true

    override x.Invoke (sender, e) =
        editor <- sender
        if editor.fsSession.IsNone then
            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is () && not editor.SelectionExists ->
                     e.Ignore <- complete ()
                | _ -> ()
