
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

    override x.Invoke (_, e) =
        let editor = far.Editor

        let menu = far.CreateMenu ()
        menu.Title <- "F#"
        menu.doAction [|
            // all menus
            yield "&1. Interactive", (fun () -> FarInteractive(getMainSession()).Open ())
            yield "&0. Sessions...", showSessions
            // editor with F#
            if e.From = ModuleToolOptions.Editor && isFSharpFileName editor.FileName then
                // all F# files; load interactive, too, i.e. load header then type
                yield "&L. Load", (fun () -> Editor.load editor)
                yield "&T. Tips", (fun () -> Editor.tips editor)
                // non interactive
                // - skip checks for interactive
                if editor.fsSession.IsNone then
                    yield "&C. Check", (fun () -> Editor.check editor)
                    if editor.fsErrors.IsSome then
                        yield "&E. Errors", (fun () -> Editor.showErrors editor)
                    yield "&F. Uses in file", (fun () -> Editor.usesInFile editor)
                    yield "&P. Uses in project", (fun () -> Editor.usesInProject editor)
                    yield (if editor.fsAutoTips then "&I. Disable auto tips" else "&I. Enable auto tips"), (fun () -> Editor.toggleAutoTips editor)
                    yield (if editor.fsAutoCheck then "&K. Disable auto check" else "&K. Enable auto check"), (fun () -> Editor.toggleAutoCheck editor)
        |]
