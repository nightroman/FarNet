
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

namespace FSharpFar

open FarNet
open Checker
open Session
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices
open System

type CheckMessage =
| Check
| Check2 of string

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
    FileText : string
}

type MouseMessage =
| Noop
| Move of LineArgs
| Tips of TipsArgs

[<System.Runtime.InteropServices.Guid "B7916B53-2C17-4086-8F13-5FFCF0D82900">]
[<ModuleEditor (Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FarEditor () =
    inherit ModuleEditor ()
    let mutable editor: IEditor = null

    let checkAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            let! message = inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            match message with
            | Check ->
                do! Async.Sleep 2000
                if inbox.CurrentQueueLength = 0 then
                    postEditorJob editor (fun () ->
                        inbox.Post (Check2 (editor.GetText ()))
                    )

            | Check2 text ->
                let options = editor.getOptions ()
                let check = Checker.check editor.FileName text options
                editor.fsErrors <-
                    if inbox.CurrentQueueLength > 0 then
                        None
                    else
                        let errors = check.CheckResults.Errors
                        if errors.Length = 0 then None else Some errors
                postEditorJob editor (fun () ->
                    editor.Redraw ()
                )
    })

    let mouseAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            let! message = inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            match message with
            | Noop -> ()

            | Move it ->
                do! Async.Sleep 400
                if inbox.CurrentQueueLength > 0 then () else

                let mutable autoTips = editor.fsAutoTips
                match editor.getMyErrors () with
                | None -> ()
                | Some errors ->
                    let lines =
                        errors
                        |> Array.filter (fun err ->
                            it.Index >= err.StartLineAlternate - 1 &&
                            it.Index <= err.EndLineAlternate - 1 &&
                            (it.Index > err.StartLineAlternate - 1 || it.Column >= err.StartColumn) &&
                            (it.Index < err.EndLineAlternate - 1 || it.Column <= err.EndColumn))
                        |> Array.map strErrorText
                        |> Array.distinct
                    if lines.Length > 0 then
                        autoTips <- false
                        let text = String.Join ("\r", lines)
                        postEditorJob editor (fun () ->
                            showText text "Errors"
                        )

                if autoTips then
                    match Parsing.findLongIdents (it.Column, it.Text) with
                    | None -> ()
                    | Some (column, idents) ->
                        postEditorJob editor (fun () ->
                            inbox.Post (Tips {Text = it.Text; Index = it.Index; Column = column; Idents = idents; FileText = editor.GetText ()})
                        )

            | Tips it ->
                let options = editor.getOptions ()
                let check = Checker.check editor.FileName it.FileText options
                let! tip = check.CheckResults.GetToolTipTextAlternate (it.Index + 1, it.Column + 1, it.Text, it.Idents, FSharpTokenTag.Identifier)
                let tips = Checker.strTip tip
                if tips.Length > 0 && inbox.CurrentQueueLength = 0 then
                    postEditorJob editor (fun () ->
                        showText tips "Tips"
                    )
    })

    let postNoop _ =
        mouseAgent.Post Noop

(*
    https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
    old EditorTests.fs(265) they use [], "" instead of names, so do we.
    new Use FsAutoComplete way.
*)
    let complete () =
        use progress = new Progress "Checking..."

        // skip out of text
        let caret = editor.Caret
        let line = editor.[caret.Y]
        if caret.X = 0 || caret.X > line.Length then false
        else

        // skip no solid base
        //TODO complete parameters -- x (y, [Tab] -- https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information
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

        let check = Checker.check file text options

        let decs = check.CheckResults.GetDeclarationListInfo (Some check.ParseResults, caret.Y + 1, colAtEndOfPartialName, lineStr, names, residue2, always false) |> Async.RunSynchronously

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

            if isSimpleSource editor.FileName then
                editor.fsAutoCheck <- true
                editor.fsAutoTips <- true

            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is () && not editor.SelectionExists ->
                     e.Ignore <- complete ()
                | _ -> ()

            editor.Changed.Add (fun _ ->
                editor.fsErrors <- None
                if editor.fsAutoCheck then
                    checkAgent.Post Check
            )

            editor.MouseDoubleClick.Add postNoop
            editor.MouseClick.Add postNoop
            editor.MouseWheel.Add postNoop
            editor.MouseMove.Add <| fun e ->
                mouseAgent.Post (
                    if e.Mouse.Is () then
                        let pos = editor.ConvertPointScreenToEditor e.Mouse.Where
                        if pos.Y < editor.Count then
                            let line = editor.[pos.Y]
                            if pos.X < line.Length then
                                Move {Text = line.Text; Index = pos.Y; Column = pos.X}
                            else Noop
                        else Noop
                    else Noop
                )
