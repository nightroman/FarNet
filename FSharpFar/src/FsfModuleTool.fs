
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open System
open System.IO
open Interactive
open Session
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

[<System.Runtime.InteropServices.Guid("65bd5625-769a-4253-8fde-ffcc3f72489d")>]
[<ModuleTool(Name = "FSharpFar", Options = ModuleToolOptions.AllMenus)>]
type FsfModuleTool() =
    inherit ModuleTool()

    let mutable editor:IEditor = null

    let showSessions() =
        let menu = far.CreateMenu()
        menu.Title <- "F# sessions"
        menu.Bottom <- "Enter, Del, F4"
        menu.AddKey(KeyCode.Delete)
        menu.AddKey(KeyCode.F4)

        let mutable loop = true
        while loop do
            loop <- List.toArray Session.Sessions |> menu.showItemKey (fun ses -> ses.DisplayName) (fun ses key ->
                match key.VirtualKeyCode with
                | KeyCode.Delete ->
                    ses.Close()
                    not Session.Sessions.IsEmpty
                | KeyCode.F4 ->
                    let editor = far.CreateEditor()
                    editor.FileName <- ses.ConfigFile
                    editor.Open()
                    false
                | _ ->
                    Interactive(ses).Open()
                    false
            )

    let load() =
        editor.Save()

        let ses = getMainSession()
        let temp = far.TempName("F#")
        let writer = new StreamWriter(temp)

        doEval writer (fun () -> ses.EvalScript(writer, editor.FileName))

        writer.Close()
        showTempFile temp "F# Output"

    let showErrors() =
        let errors = editor.fsErrors.Value

        let menu = far.CreateMenu()
        menu.Title <- "F# errors"

        errors |> menu.showItem strErrorLine (fun error ->
            editor.GoTo(error.StartColumn, error.StartLineAlternate - 1)
            editor.Redraw()
        )

    let check() =
        use progress = new UseProgress("Checking...")

        let config = editor.fsConfig()
        let file = editor.FileName
        let text = editor.GetText()

        let parseResults, checkResults = Checker.check file text config
        let errors = checkResults.Errors

        progress.Done()

        if errors.Length = 0 then
            editor.fsErrors <- None
            far.Message("No errors", "F#")
        else
            editor.fsErrors <- Some errors
            showErrors()

    let tips() =
        use progress = new UseProgress("Getting tips...")

        let caret = editor.Caret
        let lineText = editor.[caret.Y].Text

        // find end of name
        let mutable nameEnd = caret.X
        if nameEnd >= lineText.Length then
            nameEnd <- lineText.Length - 1
        if nameEnd < 0 then () else

        while nameEnd < lineText.Length && isIdentChar lineText.[nameEnd] do
            nameEnd <- nameEnd + 1

        // find start of name
        let mutable nameStart = nameEnd
        while nameStart > 1 && isIdentChar lineText.[nameStart - 1] do
            nameStart <- nameStart - 1
        let name = lineText.Substring(nameStart, nameEnd - nameStart)
        if name.Length = 0 then () else

        let config = editor.fsConfig()
        let file = editor.FileName
        let text = editor.GetText()

        let parseResults, checkResults = Checker.check file text config
        let tip = checkResults.GetToolTipTextAlternate(caret.Y + 1, nameEnd, lineText, [name], FSharpTokenTag.Identifier) |> Async.RunSynchronously

        progress.Done()

        far.Message(Checker.strTip tip, "Tips", MessageOptions.LeftAligned) |> ignore

    override x.Invoke(_, e) =
        editor <- far.Editor

        let menu = far.CreateMenu()
        menu.Title <- "F#"

        menu.doAction [|
            // all menus
            yield "&1. Interactive", (fun() -> Interactive(getMainSession()).Open())
            yield "&0. Sessions...", showSessions
            // editor with F#
            if e.From = ModuleToolOptions.Editor && isFSharpFileName editor.FileName then
                // all F# files; load interactive, too, i.e. load header then type
                yield "&L. Load", load
                yield "&T. Tips", tips
                // non interactive
                // - skip checks for interactive because `use` file is used as load, not `use`
                if editor.fsSession.IsNone then
                    yield "&C. Check", check
                    if editor.fsErrors.IsSome then
                        yield "&E. Errors", showErrors
        |]
