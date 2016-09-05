
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open System
open System.IO
open Interactive
open Session

[<System.Runtime.InteropServices.Guid("65bd5625-769a-4253-8fde-ffcc3f72489d")>]
[<ModuleTool(Name = "FSharpFar", Options = ModuleToolOptions.AllMenus)>]
type FsfModuleTool() =
    inherit ModuleTool()

    let showSessions _ =
        let menu = far.CreateMenu()
        menu.Title <- "F# sessions"
        menu.Bottom <- "Enter, Del, F4"
        menu.AddKey(KeyCode.Delete)
        menu.AddKey(KeyCode.F4)

        let rec loop() =
            menu.Items.Clear()
            for session in Session.Sessions do
                menu.Add(session.DisplayName).Data <- session

            //! data = null on Del in empty menu
            if not (menu.Show()) || menu.SelectedData = null then () else

            let ses = menu.SelectedData :?> Session

            match menu.Key.VirtualKeyCode with
            | KeyCode.Delete ->
                ses.Close()
                if Session.Sessions.IsEmpty then
                    ()
                else
                    loop()
            | KeyCode.F4 ->
                let editor = far.CreateEditor()
                editor.FileName <- ses.ConfigFile
                editor.Open()
            | _ ->
                Interactive(ses).Open()
        loop()

    let execute() =
        let editor = far.Editor
        editor.Save()

        let ses = getMainSession()
        let temp = far.TempName("F#")
        let writer = new StreamWriter(temp)

        doEval writer (fun () -> ses.EvalScript(writer, editor.FileName))

        writer.Close()
        showTempFile temp "F# Output"

    override x.Invoke(_, e) =
        let menu = far.CreateMenu()
        menu.Title <- "F#"
        menu.Add("&1. Interactive") |> ignore
        menu.Add("&0. Sessions...") |> ignore
        if e.From = ModuleToolOptions.Editor && isScriptFileName far.Editor.FileName then
            menu.Add("&L. Load") |> ignore

        let _ = menu.Show()

        match menu.Selected with
        | 0 -> Interactive(getMainSession()).Open()
        | 1 -> showSessions()
        | 2 -> execute()
        | _ -> ()
