
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

//TODO https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information

namespace FSharpFar

open FarNet
open System
open Checker
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices

type LineArgs = {
    Text : string
    Index : int
    Column : int
}

type TipsArgs = {
    Text : string
    Index : int
    Column : int
    Idents : string list
    FileName : string
    FileText : string
}

type InboxMessage =
| Noop
| Move of LineArgs
| Tips of TipsArgs

[<System.Runtime.InteropServices.Guid "B7916B53-2C17-4086-8F13-5FFCF0D82900">]
[<ModuleEditor (Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FarEditor () =
    inherit ModuleEditor ()
    let mutable editor: IEditor = null

    let inbox = MailboxProcessor.Start (fun inbox ->
        let rec loop () = async {
            let! msg = inbox.Receive ()

            if not editor.fsAutoTips || inbox.CurrentQueueLength > 0 then
                return! loop ()

            match msg with
            | Noop -> ()

            | Move it ->
                do! Async.Sleep 200
                if inbox.CurrentQueueLength = 0 then

                    match Parsing.findLongIdents (it.Column, it.Text) with
                    | None -> ()
                    | Some (column, idents) ->
                        postEditorJob editor (fun () ->
                            let file = editor.FileName
                            let text = editor.GetText ()
                            inbox.Post (Tips {Text = it.Text; Index = it.Index; Column = column; Idents = idents; FileName = file; FileText = text})
                        )

            | Tips it ->
                let options = editor.getOptions ()
                let check = Checker.check it.FileName it.FileText options
                let! tip = check.CheckResults.GetToolTipTextAlternate (it.Index + 1, it.Column + 1, it.Text, it.Idents, FSharpTokenTag.Identifier)

                let tips = Checker.strTip tip
                if tips.Length > 0 && inbox.CurrentQueueLength = 0 then
                    postEditorJob editor (fun () ->
                        showText tips "Tips"
                    )

            return! loop ()
        }
        loop ()
    )

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

    override x.Invoke (sender, _) =
        editor <- sender
        if editor.fsSession.IsNone then
            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is () && not editor.SelectionExists ->
                     e.Ignore <- complete ()
                | _ -> ()
            editor.MouseDoubleClick.Add (fun _ -> inbox.Post Noop)
            editor.MouseClick.Add (fun _ -> inbox.Post Noop)
            editor.MouseWheel.Add (fun _ -> inbox.Post Noop)
            editor.MouseMove.Add (fun e ->
                let pos = editor.ConvertPointScreenToEditor e.Mouse.Where
                if pos.Y < editor.Count then
                    let line = editor.[pos.Y]
                    if pos.X < line.Length then
                        inbox.Post (Move {Text = line.Text; Index = pos.Y; Column = pos.X})
                    else
                        inbox.Post Noop
                else
                    inbox.Post Noop
                )
