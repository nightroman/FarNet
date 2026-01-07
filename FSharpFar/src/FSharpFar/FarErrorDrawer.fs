namespace FSharpFar
open FarNet
open FSharp.Compiler.Diagnostics

[<ModuleDrawer(Name = "F# errors", Mask = "*.fs;*.fsx;*.fsscript", Id = "D122FBB8-26FA-4873-8245-A617CE200BCF")>]
type FarErrorDrawer() =
    inherit ModuleDrawer()

    override _.Invoke(editor, e) =
        match editor.MyFileErrors() with
        | None -> ()
        | Some errors ->

        let sets = Settings.Default.GetData()

        for line in e.Lines do
            for err in errors do
                if line.Index >= err.StartLine - 1 && line.Index <= err.EndLine - 1 then
                    let st, en =
                        if line.Index = err.StartLine - 1 then
                            err.StartColumn, (if line.Index = err.EndLine - 1 then err.EndColumn else line.Length)
                        elif line.Index = err.EndLine - 1 then
                            e.StartChar, err.EndColumn
                        else
                            e.StartChar, e.StartChar + 1
                    if st < en then
                        let fg, bg =
                            match err.Severity with
                            | FSharpDiagnosticSeverity.Error -> sets.ErrorForegroundColor, sets.ErrorBackgroundColor
                            | FSharpDiagnosticSeverity.Warning -> sets.WarningForegroundColor, sets.WarningBackgroundColor
                            | FSharpDiagnosticSeverity.Info -> sets.WarningForegroundColor, sets.WarningBackgroundColor
                            | FSharpDiagnosticSeverity.Hidden -> sets.WarningForegroundColor, sets.WarningBackgroundColor
                        e.Colors.Add(EditorColor(line.Index, st, en, fg, bg))
