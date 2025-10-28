﻿module FSharpFar.FarInteractive
open FarNet
open FarNet.Tools
open System
open System.IO

module private My =
    let history = HistoryLog(farLocalData + @"\InteractiveHistory.log", 1000)
    let outputMark1 = "(*("
    let outputMark2 = ")*)"
    let outputMark3 = "(**)"

type FarInteractive(session: Session) =
    inherit InteractiveEditor(far.CreateEditor(), My.history, My.outputMark1, My.outputMark2, My.outputMark3)
    do
        base.AutoSave <- true

    let session = session

    let isQuit (code: string) =
        code.AsSpan().TrimStart().StartsWith("#quit");

    override x.Invoke(code, area) =
        if area.FirstLineIndex = area.LastLineIndex && isQuit code then
            // one line with a command, for now do #quit and ignore others
            session.Close()
        else
            // eval code
            let writer = x.Editor.OpenWriter()
            Session.Eval(writer, fun () -> session.EvalInteraction(writer, code))

    override x.KeyPressed key =
        match key.VirtualKeyCode with
        | KeyCode.Tab when key.Is() && not x.Editor.SelectionExists ->
            Editor.completeBy x.Editor session.GetCompletions
        | _ ->
            base.KeyPressed key

    member x.Open() =
        let now = DateTime.Now
        let path = Path.Combine(Path.GetTempPath(), now.ToString("_yyMMdd_HHmmss") + ".interactive.fsx")
        let editor = x.Editor

        editor.FileName <- path
        editor.CodePage <- 65001
        editor.DisableHistory <- true
        editor.ExpandTabs <- ExpandTabsMode.New
        editor.Title <- $"F# {Path.GetFileName session.ConfigFile} {Path.GetFileName path}"
        editor.Switching <- Switching.Disabled
        editor.DeleteSource <- DeleteSource.File

        // connect session
        editor.MySession <- Some session
        let onSessionClose = session.OnClose.Subscribe(fun () -> if editor.IsOpened then editor.Close())
        editor.Closed.Add(fun _ -> onSessionClose.Dispose())
        if session.Errors.Length > 0 then
            editor.Opened.Add(fun _ ->
                editor.Add($"{My.outputMark1}\n{session.Errors}\n{My.outputMark2}\n")
            )

        // Open. Post, to avoid modal. Use case:
        // - open session by `fs:open`
        // - it writes echo -> user screen
        // - opening from user screen is modal
        far.PostJob(fun () ->
            editor.Open()
        )
