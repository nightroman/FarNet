
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open System
open Session

[<System.Runtime.InteropServices.Guid("B7916B53-2C17-4086-8F13-5FFCF0D82900")>]
[<ModuleEditor(Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FsfModuleEditor() =
    inherit ModuleEditor()
    override x.Invoke(editor, e) =
        if editor.fsSession.IsNone then
            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when not editor.SelectionExists ->
                    if e.Key.Is() then
                        e.Ignore <- completeCode editor (fun x -> getMainSession().GetCompletions(x)) //! fun for lazy getMainSession()
                | _ -> ()
