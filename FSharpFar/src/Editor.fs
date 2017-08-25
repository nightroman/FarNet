
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Editor

open FarNet
open Options
open Session
open FsAutoComplete
open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices

let load (editor: IEditor) =
    editor.Save ()

    let file = editor.FileName
    let ses = Session.FindOrCreate (getConfigPathForFile file)
    let temp = far.TempName "F#"

    do
        use writer = new StreamWriter (temp)
        doEval writer (fun () -> ses.EvalScript (writer, file))

    showTempFile temp "F# Output"

let showErrors (editor: IEditor) =
    let errors =
        editor.fsErrors.Value
        |> Array.sortBy (fun x -> x.FileName, x.StartLineAlternate, x.StartColumn)

    let menu = far.CreateMenu ()
    menu.Title <- "F# errors"
    menu.ShowAmpersands <- true

    errors |> menu.showItem strErrorLine (fun error ->
        editor.GoTo (error.StartColumn, error.StartLineAlternate - 1)
        editor.Redraw ()
    )

let check (editor: IEditor) =
    use progress = new Progress "Checking..."

    let options = editor.getOptions ()
    let file = editor.FileName
    let text = editor.GetText ()

    let check =
        Checker.check file text options
        |> Async.RunSynchronously

    let errors = check.CheckResults.Errors

    progress.Done ()

    if errors.Length = 0 then
        editor.fsErrors <- None
        far.Message ("No errors", "F#")
    else
        editor.fsErrors <- Some errors
        showErrors editor

let tips (editor: IEditor) =
    use progress = new Progress "Getting tips..."

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parsing.findLongIdents (caret.X, lineStr) with
    | None -> ()
    | Some (column, idents) ->

    let options = editor.getOptions ()
    let file = editor.FileName
    let text = editor.GetText ()

    let tip =
        async {
            let! check = Checker.check file text options
            return! check.CheckResults.GetToolTipText (caret.Y + 1, column + 1, lineStr, idents, FSharpTokenTag.Identifier) //??GetToolTipTextAlternate
        }
        |> Async.RunSynchronously

    progress.Done ()

    showText (Checker.strTip tip) "Tips"

let usesInFile (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parsing.findLongIdents (caret.X, lineStr) with
    | None -> ()
    | Some (col, identIsland) ->

    let options = editor.getOptions ()
    let file = editor.FileName
    let text = editor.GetText ()

    let uses =
        async {
            let! check = Checker.check file text options
            let! symboluse = check.CheckResults.GetSymbolUseAtLocation (caret.Y + 1, col + 1, lineStr, identIsland)
            match symboluse with
            | None ->
                return None
            | Some symboluse ->
                let! uses = check.CheckResults.GetUsesOfSymbolInFile symboluse.Symbol
                return Some uses
        }
        |> Async.RunSynchronously

    match uses with
    | None -> ()
    | Some uses ->

    progress.Done ()

    let menu = far.CreateMenu ()
    menu.Title <- "F# uses"
    menu.ShowAmpersands <- true

    let strUseLine (x: FSharpSymbolUse) =
        let range = x.RangeAlternate
        sprintf "%s(%d,%d): %s" (Path.GetFileName x.FileName) range.StartLine (range.StartColumn + 1) editor.[range.StartLine - 1].Text

    uses |> menu.showItem strUseLine (fun x ->
        let range = x.RangeAlternate
        editor.GoTo (range.StartColumn, range.StartLine - 1)
        editor.Redraw ()
    )

let usesInProject (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    editor.Save()

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parsing.findLongIdents (caret.X, lineStr) with
    | None -> ()
    | Some (col, identIsland) ->

    let options = editor.getOptions ()
    let file = editor.FileName
    let text = editor.GetText ()

    let uses =
        async {
            let! check = Checker.check file text options
            let! sym = check.CheckResults.GetSymbolUseAtLocation (caret.Y + 1, col + 1, lineStr, identIsland)
            match sym with
            | None ->
                return None
            | Some sym ->
                let! pr = check.Checker.ParseAndCheckProject check.Options
                let! uses = pr.GetUsesOfSymbol sym.Symbol
                return Some (uses, sym)
        }
        |> Async.RunSynchronously

    match uses with
    | None -> ()
    | Some (uses, sym) ->

    progress.Done ()

    let menu = far.CreateMenu ()
    menu.Title <- "F# uses"
    menu.ShowAmpersands <- true

    let mutable map = Map.empty
    let lines file =
        match map.TryFind file with
        | Some r -> r
        | _ ->
            let r = File.ReadAllLines file
            map <- map.Add (file, r)
            r

    use writer = new StringWriter ()
    for x in uses do
        let lines = lines x.FileName
        let range = x.RangeAlternate
        fprintfn writer "%s(%d,%d): %s" x.FileName range.StartLine (range.StartColumn + 1) lines.[range.StartLine - 1]

    showTempText (writer.ToString ()) ("F# Uses " + sym.Symbol.FullName)

let toggleAutoTips (editor: IEditor) =
    editor.fsAutoTips <- not editor.fsAutoTips

let toggleAutoCheck (editor: IEditor) =
    editor.fsAutoCheck <- not editor.fsAutoCheck

(*
    https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
    old EditorTests.fs(265) they use [], "" instead of names, so do we.
    new Use FsAutoComplete way.
*)
let complete (editor: IEditor) =
    use progress = new Progress "Checking..."

    // skip out of text
    let caret = editor.Caret
    let line = editor.[caret.Y]
    if caret.X = 0 || caret.X > line.Length then false
    else

    // skip no solid base
    //TODO complete parameters -- x (y, [Tab] -- https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information
    let lineStr = line.Text
    if Char.IsWhiteSpace lineStr.[caret.X - 1] then false
    else

    // parse
    let names, residue = Parsing.findLongIdentsAndResidue (caret.X, lineStr)

(*
    _160922_160602
    Complete `x.ToString().C` incorrectly gives all globals.
    But complete `x.ToString().` gives string members.
    Let's reduce to the working fine case.
*)
    let mutable residue2 = residue
    let mutable colAtEndOfPartialName = caret.X + 1
    let isDot () =
        let i = caret.X - 1 - residue.Length
        i > 0 && lineStr.[i] = '.'
    if residue.Length > 0 && names.IsEmpty && isDot () then
        residue2 <- ""
        colAtEndOfPartialName <- colAtEndOfPartialName - residue.Length

    let options = editor.getOptions ()
    let file = editor.FileName
    let text = editor.GetText ()

    let decs =
        async {
            let! check = Checker.check file text options
            return! check.CheckResults.GetDeclarationListInfo (Some check.ParseResults, caret.Y + 1, colAtEndOfPartialName, lineStr, names, residue2, always [])
        }
        |> Async.RunSynchronously

    let completions =
        decs.Items
        |> Seq.map (fun item -> item.Name)
        |> Seq.filter (fun name -> name.StartsWith residue)

    progress.Done ()

    completeLine editor.Line (caret.X - residue.Length) residue.Length completions
    editor.Redraw ()
    true
