﻿namespace FSharpFar
open FarNet
open FarInteractive

[<ModuleTool(Name = "FSharpFar", Options = ModuleToolOptions.F11Menus, Id = "65bd5625-769a-4253-8fde-ffcc3f72489d")>]
type FarTool() =
    inherit ModuleTool()

    let openSession () =
        FarInteractive(Session.DefaultSession()).Open()

    let showSessions () =
        let menu = far.CreateListMenu(Title = "F# sessions", Bottom = "Enter, Del, F4", ShowAmpersands = true, UsualMargins = true)
        menu.AddKey KeyCode.Delete
        menu.AddKey KeyCode.F4

        let mutable loop = true
        while loop do
            loop <- Session.Sessions |> menu.ShowItemsWithKeys(fun ses -> ses.DisplayName) (fun ses key ->
                match key.VirtualKeyCode with
                | KeyCode.Delete ->
                    ses.Close()
                    //! do not close even empty, keep predictable for typing
                    true
                | KeyCode.F4 ->
                    let editor = far.CreateEditor()
                    editor.FileName <- ses.ConfigFile
                    editor.Open()
                    false
                | _ ->
                    FarInteractive(ses).Open()
                    false
            )

    override __.Invoke(_, e) =
        let editor = far.Editor

        let menu = far.CreateMenu(Title = "F#")
        menu.ShowActions [
            // all menus
            "&1. Interactive", openSession
            "&0. Sessions...", showSessions
            // editor with F#
            if e.From = ModuleToolOptions.Editor && isFSharpFileName editor.FileName then
                // all F# files; load interactive, too, i.e. load header then type
                "&L. Load", (fun _ -> Editor.load editor)
                "&T. Tips", (fun _ -> Editor.tips editor)
                // non interactive
                // - skip checks for interactive
                if editor.MySession.IsNone then
                    "&C. Check", (fun _ -> Editor.check editor)
                    if editor.MyErrors.IsSome then
                        "&E. Errors", (fun _ -> Editor.showErrors editor)
                    "&F. Uses in file", (fun _ -> Editor.usesInFile editor)
                    "&P. Uses in project", (fun _ -> Editor.usesInProject editor)
                    (if Workings.Default.GetData().AutoTips then "&I. Disable auto tips" else "&I. Enable auto tips"), (fun _ -> Editor.toggleAutoTips editor)
                    (if Workings.Default.GetData().AutoCheck then "&K. Disable auto check" else "&K. Enable auto check"), (fun _ -> Editor.toggleAutoCheck editor)
        ]
