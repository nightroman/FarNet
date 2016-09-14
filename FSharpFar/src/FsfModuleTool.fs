
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open System
open System.IO
open Config
open Interactive
open Session
open Microsoft.FSharp.Compiler

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
        let errors = editor.Data.[DataKey.errors] :?> FSharpErrorInfo[]

        let menu = far.CreateMenu()
        menu.Title <- "F# errors"

        errors |> menu.showItem strErrorLine (fun error ->
            editor.GoTo(error.StartColumn, error.StartLineAlternate - 1)
            editor.Redraw()
        )

    let check() =
        use progress = new UseProgress("Checking...")

        let file = editor.FileName
        let text = editor.GetText()

        let config =
            match editor.Data.[DataKey.session] with
            | :? Session as ses ->
                //TODO interactive is not checked but will be needed for definitions
                ses.Config
            | _ ->
                let dir = Path.GetDirectoryName(file)
                let ini = Directory.GetFiles(dir, "*.fs.ini")
                match ini with
                | [|configFile|] ->
                    getConfigurationFromFile configFile
                | _ ->
                    getMainSession().Config

        let parseResults, checkResults = Checker.check file text config

        let errors = checkResults.Errors

        progress.Done()
        if errors.Length = 0 then
            far.Message("No errors", "F#")
            editor.Data.Remove(DataKey.errors)
        else
            editor.Data.[DataKey.errors] <- errors
            showErrors()

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
                // non interactive
                // - skip checks for interactive because `use` file is used as load, not `use`
                if editor.Data.[DataKey.session] = null then
                    yield "&C. Check", check
                    if editor.Data.ContainsKey(DataKey.errors) then
                        yield "&E. Errors", showErrors
        |]
