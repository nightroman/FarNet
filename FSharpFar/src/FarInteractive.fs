
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FarInteractive

open FarNet
open FarNet.Tools
open Command
open Session
open System
open System.IO

type FarInteractive(session: Session) =
    inherit InteractiveEditor (far.CreateEditor (), "(*(", ")*)", "(**)")
    let session = session

    override x.Invoke (code, area) =
        // one line with a command; for now do #quit and ignore others
        if area.HeadLineIndex = area.LastLineIndex && (match parseCommand code with Quit -> true | _ -> false) then
            session.Close ()

        // eval code
        else
        let writer = x.Editor.OpenWriter ()
        doEval writer (fun () -> session.EvalInteraction (writer, code))

    override x.KeyPressed key =
        match key.VirtualKeyCode with
        | KeyCode.Tab when key.Is () && not x.Editor.SelectionExists ->
            completeCode x.Editor session.GetCompletions
        | _ ->
            base.KeyPressed key

    member x.Open () =
        let path = Path.Combine (fsfLocalData (), (DateTime.Now.ToString "_yyMMdd_HHmmss") + ".interactive.fsx")
        let editor = x.Editor
        
        editor.FileName <- path
        editor.CodePage <- 65001
        editor.DisableHistory <- true
        editor.Title <- sprintf "F# %s %s" (Path.GetFileName session.ConfigFile) (Path.GetFileName path)

        // attach to session
        editor.fsSession <- Some session
        let onSessionClose = Handler<unit> (fun _ _ ->
            if editor.IsOpened then
                editor.Close ()
        )
        session.OnClose.AddHandler onSessionClose
        editor.Closed.Add <| fun _ ->
            session.OnClose.RemoveHandler onSessionClose

        // Open. Post, to avoid modal. Use case:
        // - open session by `fs: //open`
        // - it writes echo -> user screen
        // - opening from user screen is modal
        far.PostJob (fun _ ->
            editor.Open()
        )

        // Show issues. Post, for legit modal cases like opening from a dialog.
        // We want some job to be done after opening in any case.
        far.PostJob (fun () ->
            if session.Errors.Length > 0 then
                showTempText session.Errors ("F# Errors " + session.DisplayName)
        )
