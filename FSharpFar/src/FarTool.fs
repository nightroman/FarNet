
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open Options
open Session
open FarInteractive
open FsAutoComplete
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices

[<System.Runtime.InteropServices.Guid "65bd5625-769a-4253-8fde-ffcc3f72489d">]
[<ModuleTool (Name = "FSharpFar", Options = ModuleToolOptions.F11Menus)>]
type FarTool () =
    inherit ModuleTool ()

    let mutable editor:IEditor = null

    let showSessions () =
        let menu = far.CreateMenu ()
        menu.Title <- "F# sessions"
        menu.Bottom <- "Enter, Del, F4"
        menu.AddKey KeyCode.Delete
        menu.AddKey KeyCode.F4

        let mutable loop = true
        while loop do
            loop <- List.toArray Session.Sessions |> menu.showItemKey (fun ses -> ses.DisplayName) (fun ses key ->
                match key.VirtualKeyCode with
                | KeyCode.Delete ->
                    ses.Close ()
                    not Session.Sessions.IsEmpty
                | KeyCode.F4 ->
                    let editor = far.CreateEditor ()
                    editor.FileName <- ses.ConfigFile
                    editor.Open ()
                    false
                | _ ->
                    FarInteractive(ses).Open ()
                    false
            )

    let load () =
        editor.Save ()

        let file = editor.FileName
        let ses = Session.FindOrCreate (getConfigPathForFile file)
        let temp = far.TempName "F#"

        do
            use writer = new StreamWriter (temp)
            doEval writer (fun () -> ses.EvalScript (writer, file))

        showTempFile temp "F# Output"

    let showErrors () =
        let errors = editor.fsErrors.Value

        let menu = far.CreateMenu ()
        menu.Title <- "F# errors"
        menu.ShowAmpersands <- true

        errors |> menu.showItem strErrorLine (fun error ->
            editor.GoTo (error.StartColumn, error.StartLineAlternate - 1)
            editor.Redraw ()
        )

    let check () =
        use progress = new Progress "Checking..."

        let options = editor.getOptions ()
        let file = editor.FileName
        let text = editor.GetText ()

        let check = Checker.check file text options
        let errors = check.CheckResults.Errors

        progress.Done ()

        if errors.Length = 0 then
            editor.fsErrors <- None
            far.Message ("No errors", "F#")
        else
            editor.fsErrors <- Some errors
            showErrors ()

    let tips () =
        use progress = new Progress "Getting tips..."

        let caret = editor.Caret
        let lineStr = editor.[caret.Y].Text

        match Parsing.findLongIdents (caret.X, lineStr) with
        | None -> ()
        | Some (column, idents) ->

        let options = editor.getOptions ()
        let file = editor.FileName
        let text = editor.GetText ()

        let check = Checker.check file text options
        let tip = check.CheckResults.GetToolTipTextAlternate (caret.Y + 1, column + 1, lineStr, idents, FSharpTokenTag.Identifier) |> Async.RunSynchronously

        progress.Done ()

        showText (Checker.strTip tip) "Tips"

    let usesInFile () =
        use progress = new Progress "Getting uses..."

        let caret = editor.Caret
        let lineStr = editor.[caret.Y].Text

        match Parsing.findLongIdents (caret.X, lineStr) with
        | None -> ()
        | Some (col, identIsland) ->

        let options = editor.getOptions ()
        let file = editor.FileName
        let text = editor.GetText ()

        let check = Checker.check file text options
        let symboluse = check.CheckResults.GetSymbolUseAtLocation (caret.Y + 1, col + 1, lineStr, identIsland) |> Async.RunSynchronously

        match symboluse with
        | None -> ()
        | Some symboluse ->

        let uses = check.CheckResults.GetUsesOfSymbolInFile symboluse.Symbol |> Async.RunSynchronously

        progress.Done ()

        let menu = far.CreateMenu ()
        menu.Title <- "F# uses"
        menu.ShowAmpersands <- true

        let strUseLine (x: FSharpSymbolUse) =
            let range = x.RangeAlternate
            sprintf "%s(%d,%d): %s" (Path.GetFileName x.FileName) range.StartLine (range.StartColumn + 1) editor.[range.StartLine - 1].Text

        uses |> menu.showItem strUseLine (fun x ->
            let range = x.RangeAlternate
            editor.GoTo (range.StartColumn, range.StartLine - 1)
            editor.Redraw ()
        )

    let usesInProject () =
        use progress = new Progress "Getting uses..."

        editor.Save()

        let caret = editor.Caret
        let lineStr = editor.[caret.Y].Text

        match Parsing.findLongIdents (caret.X, lineStr) with
        | None -> ()
        | Some (col, identIsland) ->

        let options = editor.getOptions ()
        let file = editor.FileName
        let text = editor.GetText ()

        let check = Checker.check file text options
        let sym = check.CheckResults.GetSymbolUseAtLocation (caret.Y + 1, col + 1, lineStr, identIsland) |> Async.RunSynchronously

        match sym with
        | None -> ()
        | Some sym ->

        let pr = check.Checker.ParseAndCheckProject check.Options |> Async.RunSynchronously
        let uses = pr.GetUsesOfSymbol sym.Symbol |> Async.RunSynchronously

        progress.Done ()

        let menu = far.CreateMenu ()
        menu.Title <- "F# uses"
        menu.ShowAmpersands <- true

        let mutable map = Map.empty<string, string []>
        let lines file =
            match map.TryFind file with
            | Some r -> r
            | _ ->
                let r = File.ReadAllLines file
                map <- map.Add (file, r)
                r

        use writer = new StringWriter ()
        for x in uses do
            let lines = lines x.FileName
            let range = x.RangeAlternate
            fprintfn writer "%s(%d,%d): %s" x.FileName range.StartLine (range.StartColumn + 1) lines.[range.StartLine - 1]

        showTempText (writer.ToString ()) ("F# Uses " + sym.Symbol.FullName)

    let toggleAutoTips () =
        editor.fsAutoTips <- not editor.fsAutoTips

    let toggleAutoCheck () =
        editor.fsAutoCheck <- not editor.fsAutoCheck

    override x.Invoke (_, e) =
        editor <- far.Editor

        let menu = far.CreateMenu ()
        menu.Title <- "F#"

        menu.doAction [|
            // all menus
            yield "&1. Interactive", (fun () -> FarInteractive(getMainSession()).Open ())
            yield "&0. Sessions...", showSessions
            // editor with F#
            if e.From = ModuleToolOptions.Editor && isFSharpFileName editor.FileName then
                // all F# files; load interactive, too, i.e. load header then type
                yield "&L. Load", load
                yield "&T. Tips", tips
                // non interactive
                // - skip checks for interactive
                if editor.fsSession.IsNone then
                    yield "&C. Check", check
                    if editor.fsErrors.IsSome then
                        yield "&E. Errors", showErrors
                    yield "&F. Uses in file", usesInFile
                    yield "&P. Uses in project", usesInProject
                    yield (if editor.fsAutoTips then "&I. Disable auto tips" else "&I. Enable auto tips"), toggleAutoTips
                    yield (if editor.fsAutoCheck then "&K. Disable auto check" else "&K. Enable auto check"), toggleAutoCheck
        |]
