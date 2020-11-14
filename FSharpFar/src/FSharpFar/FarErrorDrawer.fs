namespace FSharpFar
open FarNet
open FSharp.Compiler.SourceCodeServices

[<ModuleDrawer (Name = "F# errors", Mask = "*.fs;*.fsx;*.fsscript")>]
[<Guid "D122FBB8-26FA-4873-8245-A617CE200BCF">]
type FarErrorDrawer () =
    inherit ModuleDrawer ()

    let bgError = Settings.Default.ErrorBackgroundColor
    let fgError = Settings.Default.ErrorForegroundColor
    let bgWarning = Settings.Default.WarningBackgroundColor
    let fgWarning = Settings.Default.WarningForegroundColor

    override __.Invoke (editor, e) =
        match editor.MyFileErrors () with
        | None -> ()
        | Some errors ->

        let isChecking = editor.MyChecking
        for line in e.Lines do
            for err in errors do
                if line.Index >= err.StartLineAlternate - 1 && line.Index <= err.EndLineAlternate - 1 then
                    let st, en =
                        if isChecking then
                            e.StartChar, e.StartChar + 1
                        elif line.Index = err.StartLineAlternate - 1 then
                            err.StartColumn, (if line.Index = err.EndLineAlternate - 1 then err.EndColumn else line.Length)
                        elif line.Index = err.EndLineAlternate - 1 then
                            e.StartChar, err.EndColumn
                        else
                            e.StartChar, e.StartChar + 1
                    if st < en then
                        let fg, bg =
                            match err.Severity with
                            | FSharpDiagnosticSeverity.Error -> fgError, bgError
                            | FSharpDiagnosticSeverity.Warning -> fgWarning, bgWarning
                            | FSharpDiagnosticSeverity.Info -> fgWarning, bgWarning
                            | FSharpDiagnosticSeverity.Hidden -> fgWarning, bgWarning
                        e.Colors.Add (EditorColor (line.Index, st, en, fg, bg))
