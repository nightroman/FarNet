
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open System

[<System.Runtime.InteropServices.Guid "D122FBB8-26FA-4873-8245-A617CE200BCF">]
[<ModuleDrawer (Name = "F# errors", Mask = "*.fs;*.fsx;*.fsscript")>]
type FarErrorDrawer () =
    inherit ModuleDrawer ()

    override x.Invoke (editor, e) =
        match editor.getMyErrors () with
        | None -> ()
        | Some errors ->

        for line in e.Lines do
            for err in errors do
                if line.Index >= err.StartLineAlternate - 1 && line.Index <= err.EndLineAlternate - 1 then
                    let st, en =
                        if line.Index = err.StartLineAlternate - 1 then
                            err.StartColumn, (if line.Index = err.EndLineAlternate - 1 then err.EndColumn else line.Length)
                        elif line.Index = err.EndLineAlternate - 1 then
                            e.StartChar, err.EndColumn
                        else
                            e.StartChar, e.StartChar + 1
                    if st < en then
                        e.Colors.Add (EditorColor (line.Index, st, en, ConsoleColor.White, ConsoleColor.Red))
