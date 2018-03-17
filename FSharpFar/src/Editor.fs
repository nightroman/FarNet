module FSharpFar.Editor
open FarNet
open Config
open Session
open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler

let load (editor: IEditor) =
    editor.Save ()

    let file = editor.FileName
    let ses = Session.GetOrCreate (getConfigPathForFile file)
    let temp = far.TempName "F#"

    do
        use writer = new StreamWriter (temp)

        // session errors first or issues may look cryptic
        if ses.Errors.Length > 0 then
            writer.Write ses.Errors

        // eval anyway, session errors may be warnings
        doEval writer (fun _ -> ses.EvalScript (writer, file))

    showTempFile temp "F# Output"

let showErrors (editor: IEditor) =
    let errors =
        editor.MyErrors.Value
        |> Array.sortBy (fun x -> x.FileName, x.StartLineAlternate, x.StartColumn)

    let menu = far.CreateListMenu (Title = "F# errors", ShowAmpersands = true, UsualMargins = true, IncrementalOptions = PatternOptions.Substring)

    errors |> menu.ShowItems strErrorLine (fun error ->
        editor.GoTo (error.StartColumn, error.StartLineAlternate - 1)
        editor.Redraw ()
    )

let check (editor: IEditor) =
    use progress = new Progress "Checking..."

    let config = editor.MyConfig ()
    let file = editor.FileName
    let text = editor.GetText ()

    let check =
        Checker.check file text config
        |> Async.RunSynchronously

    let errors = check.CheckResults.Errors

    progress.Done ()

    if errors.Length = 0 then
        editor.MyErrors <- None
        far.Message ("No errors", "F#")
    else
        editor.MyErrors <- Some errors
        showErrors editor

let tips (editor: IEditor) =
    use progress = new Progress "Getting tips..."

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (column, idents) ->

    let config = editor.MyConfig ()
    let file = editor.FileName
    let text = editor.GetText ()

    let tip =
        async {
            let! check = Checker.check file text config
            return! check.CheckResults.GetToolTipText (caret.Y + 1, column + 1, lineStr, idents, FSharpTokenTag.Identifier)
        }
        |> Async.RunSynchronously

    progress.Done ()

    showTempText (Tips.format tip true) (String.Join (".", List.toArray idents))

let usesInFile (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (col, identIsland) ->

    let config = editor.MyConfig ()
    let file = editor.FileName
    let text = editor.GetText ()

    let uses =
        async {
            let! check = Checker.check file text config
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

    let menu = far.CreateListMenu (Title = "F# uses", ShowAmpersands = true, UsualMargins = true, IncrementalOptions = PatternOptions.Substring)

    let strUseLine (x: FSharpSymbolUse) =
        let range = x.RangeAlternate
        sprintf "%s(%d,%d): %s" (Path.GetFileName x.FileName) range.StartLine (range.StartColumn + 1) editor.[range.StartLine - 1].Text

    uses |> menu.ShowItems strUseLine (fun x ->
        let range = x.RangeAlternate
        editor.GoTo (range.StartColumn, range.StartLine - 1)
        editor.Redraw ()
    )

let usesInProject (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    editor.Save()

    let caret = editor.Caret
    let lineStr = editor.[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (col, identIsland) ->

    let config = editor.MyConfig ()
    let file = editor.FileName
    let text = editor.GetText ()

    let uses =
        async {
            let! check = Checker.check file text config
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

    let fileLines =
        uses
        |> Array.map (fun x -> x.FileName)
        |> Array.distinct
        |> Array.map (fun file -> file, File.ReadAllLines file)
        |> Map.ofArray

    use writer = new StringWriter ()
    for x in uses do
        let lines = fileLines.[x.FileName]
        let range = x.RangeAlternate
        fprintfn writer "%s(%d,%d): %s" x.FileName range.StartLine (range.StartColumn + 1) lines.[range.StartLine - 1]

    progress.Done ()
    
    showTempText (writer.ToString ()) ("F# Uses " + sym.Symbol.FullName)

let toggleAutoTips (editor: IEditor) =
    editor.MyAutoTips <- not editor.MyAutoTips

let toggleAutoCheck (editor: IEditor) =
    editor.MyAutoCheck <- not editor.MyAutoCheck

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
    //! `index` is the last char index, not cursor index
    let partialName = QuickParse.GetPartialLongNameEx(lineStr, caret.X - 1) //rk new

    let config = editor.MyConfig ()
    let file = editor.FileName
    let text = editor.GetText ()

    let decs =
        async {
            let! check = Checker.check file text config
            return! check.CheckResults.GetDeclarationListInfo (Some check.ParseResults, caret.Y + 1, lineStr, partialName, always [])
        }
        |> Async.RunSynchronously

    // _160922_160602
    // KO: Complete `x.ToString().C` gives global symbols.
    // OK: Complete `x.ToString().` gives string members.
    // Let's reduce to the working case.
    // _171107
    let residue =
        if partialName.PartialIdent.Length > 0 then
            partialName.PartialIdent
        else
            let names, residue = Parser.findLongIdentsAndResidue caret.X lineStr // rk old
            residue

    let completions =
        decs.Items
        |> Seq.map (fun item -> item.Name) //?? mind NameInCode
        |> Seq.filter (fun name -> name.StartsWith (if residue.StartsWith "``" then residue.Substring 2 else residue)) //rk

    progress.Done ()

    completeLine editor.Line (caret.X - residue.Length) residue.Length completions
    editor.Redraw ()
    true

let completeBy (editor: IEditor) getCompletions =
    // skip out of text
    let line = editor.Line
    let caret = line.Caret
    if caret = 0 || caret > line.Length then false else

    // skip no solid base
    let lineStr = line.Text
    if Char.IsWhiteSpace lineStr.[caret - 1] then false else

    // parse, skip none
    let longIdent = Parser.findLongIdent caret lineStr //rk retire Parser
    if longIdent.Length = 0 then false else
    
    let name, replacementIndex =
        if lineStr.[caret - 1] = '.' then
            longIdent + ".", caret
        else
            let dot = longIdent.LastIndexOf '.'
            let start = caret - longIdent.Length
            let replacementIndex = if dot < 0 then start else start + dot + 1
            longIdent, replacementIndex

    //_161108_054202
    let name = name.Replace ("``", "")
    
    // distinct: Sys[Tab] -> several "System"
    // sort: System.[Tab] -> unsorted
    let completions =
        getCompletions name
        |> Seq.distinct
        |> Seq.sort
        |> Seq.toArray

    completeLine line replacementIndex (caret - replacementIndex) completions
    editor.Redraw ()
    true
