
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

namespace FSharpFar

open FarNet
open Session
open FarInteractive

[<System.Runtime.InteropServices.Guid "65bd5625-769a-4253-8fde-ffcc3f72489d">]
[<ModuleTool (Name = "FSharpFar", Options = ModuleToolOptions.F11Menus)>]
type FarTool () =
    inherit ModuleTool ()

    let showSessions () =
        let menu = far.CreateListMenu (Title = "F# sessions", Bottom = "Enter, Del, F4", ShowAmpersands = true, UsualMargins = true)
        menu.AddKey KeyCode.Delete
        menu.AddKey KeyCode.F4

        let mutable loop = true
        while loop do
            loop <- Session.Sessions |> menu.ShowItemsWithKeys (fun ses -> ses.DisplayName) (fun ses key ->
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

    override __.Invoke (_, e) =
        let editor = far.Editor

        let menu = far.CreateMenu (Title = "F#")
        menu.ShowActions [
            // all menus
            yield "&1. Interactive", (fun _ -> FarInteractive(getMainSession ()).Open ())
            yield "&0. Sessions...", showSessions
            // editor with F#
            if e.From = ModuleToolOptions.Editor && isFSharpFileName editor.FileName then
                // all F# files; load interactive, too, i.e. load header then type
                yield "&L. Load", (fun _ -> Editor.load editor)
                yield "&T. Tips", (fun _ -> Editor.tips editor)
                // non interactive
                // - skip checks for interactive
                if editor.MySession.IsNone then
                    yield "&C. Check", (fun _ -> Editor.check editor)
                    if editor.MyErrors.IsSome then
                        yield "&E. Errors", (fun _ -> Editor.showErrors editor)
                    yield "&F. Uses in file", (fun _ -> Editor.usesInFile editor)
                    yield "&P. Uses in project", (fun _ -> Editor.usesInProject editor)
                    yield (if editor.MyAutoTips then "&I. Disable auto tips" else "&I. Enable auto tips"), (fun _ -> Editor.toggleAutoTips editor)
                    yield (if editor.MyAutoCheck then "&K. Disable auto check" else "&K. Enable auto check"), (fun _ -> Editor.toggleAutoCheck editor)
        ]
