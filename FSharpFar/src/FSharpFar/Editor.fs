module FSharpFar.Editor
open FarNet
open System
open System.IO
open FSharp.Compiler.EditorServices
open FSharp.Compiler.Tokenization
open FSharp.Compiler.CodeAnalysis

let load (editor: IEditor) =
    editor.Save()

    let file = editor.FileName
    let ses = Session.GetOrCreate(Config.defaultFileForFile file)
    let temp = far.TempName "F#"

    do
        use writer = new StreamWriter(temp)

        // session errors first or issues may look cryptic
        if ses.Errors.Length > 0 then
            writer.Write ses.Errors

        // eval
        if ses.Ok then
            Session.Eval(writer, fun () -> ses.EvalScript(writer, file))

    showTempFile temp "F# Output"

let showErrors (editor: IEditor) =
    let errors =
        editor.MyErrors.Value
        |> Array.sortBy (fun x -> x.FileName, x.StartLine, x.StartColumn)

    let menu = far.CreateListMenu(Title = "F# errors", ShowAmpersands = true, UsualMargins = true, IncrementalOptions = PatternOptions.Substring)

    errors |> menu.ShowItems FSharpDiagnostic.strErrorLine (fun error ->
        editor.GoTo(error.StartColumn, error.StartLine - 1)
        editor.Redraw()
    )

let sourceText (editor: IEditor) =
    let n = editor.Count
    let lines = Array.zeroCreate n
    for i in 0 .. n - 1 do
        lines[i] <- editor[i].Text
    SourceText.ofLines lines

let check (editor: IEditor) =
    use progress = new Progress "Checking..."

    let config = editor.MyConfig()
    let file = editor.FileName
    let text = sourceText editor

    let check =
        Checker.check file text config
        |> Async.RunSynchronously

    let errors = check.CheckResults.Diagnostics

    progress.Done()

    if errors.Length = 0 then
        editor.MyErrors <- None
        far.Message("No errors", "F#")
    else
        editor.MyErrors <- Some errors
        showErrors editor

let tips (editor: IEditor) =
    use progress = new Progress "Getting tips..."

    let caret = editor.Caret
    let lineStr = editor[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (column, idents) ->

    let config = editor.MyConfig()
    let file = editor.FileName
    let text = sourceText editor

    let tip =
        async {
            let! check = Checker.check file text config
            return check.CheckResults.GetToolTip(caret.Y + 1, column + 1, lineStr, idents, FSharpTokenTag.Identifier)
        }
        |> Async.RunSynchronously

    progress.Done()

    showTempText (Tips.format tip true) (String.Join(".", List.toArray idents))

let usesInFile (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    let caret = editor.Caret
    let lineStr = editor[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (col, identIsland) ->

    let config = editor.MyConfig()
    let file = editor.FileName
    let text = sourceText editor

    async {
        let! check = Checker.check file text config
        match check.CheckResults.GetSymbolUseAtLocation(caret.Y + 1, col + 1, lineStr, identIsland) with
        | None ->
            return None
        | Some symboluse ->
            let uses = check.CheckResults.GetUsesOfSymbolInFile symboluse.Symbol
            return Some uses
    }
    |> Async.RunSynchronously
    |> function
    | None -> ()
    | Some uses ->

    progress.Done()

    let menu = far.CreateListMenu(Title = "F# uses", ShowAmpersands = true, UsualMargins = true, IncrementalOptions = PatternOptions.Substring)

    let strUseLine (x: FSharpSymbolUse) =
        let range = x.Range
        $"{Path.GetFileName x.FileName}({range.StartLine},{range.StartColumn + 1}): {editor[range.StartLine - 1].Text}"

    uses |> menu.ShowItems strUseLine (fun x ->
        let range = x.Range
        editor.GoTo(range.StartColumn, range.StartLine - 1)
        editor.Redraw()
    )

let usesInProject (editor: IEditor) =
    use progress = new Progress "Getting uses..."

    editor.Save()

    let caret = editor.Caret
    let lineStr = editor[caret.Y].Text

    match Parser.findLongIdents caret.X lineStr with
    | None -> ()
    | Some (col, identIsland) ->

    let config = editor.MyConfig()
    let file = editor.FileName
    let text = sourceText editor

    async {
        let! check = Checker.check file text config
        match check.CheckResults.GetSymbolUseAtLocation(caret.Y + 1, col + 1, lineStr, identIsland) with
        | None ->
            return None
        | Some sym ->
            let! pr = check.Checker.ParseAndCheckProject check.Options
            let uses = pr.GetUsesOfSymbol sym.Symbol
            return Some(uses, sym)
    }
    |> Async.RunSynchronously
    |> function
    | None -> ()
    | Some (uses, sym) ->

    let fileLines =
        uses
        |> Array.map (fun x -> x.FileName)
        |> Array.distinct
        |> Array.map (fun file -> file, File.ReadAllLines file)
        |> Map.ofArray

    use writer = new StringWriter()
    for x in uses do
        let lines = fileLines[x.FileName]
        let range = x.Range
        fprintfn writer "%s(%d,%d): %s" x.FileName range.StartLine (range.StartColumn + 1) lines[range.StartLine - 1]

    progress.Done()

    showTempText (writer.ToString()) ("F# Uses " + sym.Symbol.FullName)

let toggleAutoTips (editor: IEditor) =
    editor.MyAutoTips <- not editor.MyAutoTips

let toggleAutoCheck (editor: IEditor) =
    // toggle flag
    let isAutoCheck = not editor.MyAutoCheck
    editor.MyAutoCheck <- isAutoCheck

    // drop errors
    if not isAutoCheck then
        editor.MyErrors <- None

let fixComplete word words (ident: PartialLongName) =
    if Array.isEmpty words && ident.LastDotPos.IsNone && "__SOURCE_DIRECTORY__".StartsWith word && word.Length > 0 then
        [| "__SOURCE_DIRECTORY__" |]
    else
        // amend non-standard identifiers
        for i in 0 .. words.Length - 1 do
            let word = words[i]
            //_211111_g4 case of already quioted, e.g. ``aa-1``
            if isIdentStr word || word.StartsWith("``") && word.EndsWith("``") then
                ()
            else
                words[i] <- "``" + word + "``"
        words

// https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists
// old EditorTests.fs(265) they use [], "" instead of names, so do we.
// new Use FsAutoComplete way.
let complete (editor: IEditor) =
    use progress = new Progress "Checking..."

    // skip out of text
    let caret = editor.Caret
    let line = editor[caret.Y]
    if caret.X = 0 || caret.X > line.Length then false
    else

    // skip no solid base
    //TODO complete parameters -- x (y, [Tab] -- https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-parameter-information
    let lineStr = line.Text
    if Char.IsWhiteSpace lineStr[caret.X - 1] then false
    else
    let config = editor.MyConfig()
    let file = editor.FileName
    let text = sourceText editor

    // parse
    //! `index` is the last char index, not cursor index
    let ident = QuickParse.GetPartialLongNameEx(lineStr, caret.X - 1)

    //! https://github.com/fsharp/FSharp.Compiler.Service/issues/837
    let partialIdent =
        if ident.PartialIdent.Length > 0 then
            ident.PartialIdent
        else
            // correct PartialIdent
            QuickParse.GetPartialLongName(lineStr, caret.X - 1) |> snd

    let decs =
        async {
            let! check = Checker.check file text config
            return check.CheckResults.GetDeclarationListInfo(Some check.ParseResults, caret.Y + 1, lineStr, ident, always [])
        }
        |> Async.RunSynchronously

    let completions =
        decs.Items
        |> Array.map (fun item -> item.Name) //?? mind NameInCode
        |> Array.filter (fun name -> name.StartsWith(if partialIdent.StartsWith "``" then partialIdent.Substring 2 else partialIdent))
        |> Array.sort

    progress.Done()

    let completions = fixComplete partialIdent completions ident

    completeLine editor.Line (caret.X - partialIdent.Length) partialIdent.Length completions
    editor.Redraw()
    true

let completeBy (editor: IEditor) getCompletions =
    // skip out of text
    let line = editor.Line
    let caret = line.Caret
    if caret = 0 || caret > line.Length then false else

    // skip no solid base
    let lineStr = line.Text
    if Char.IsWhiteSpace lineStr[caret - 1] then false else

    // parse, skip none
    match Parser.tryCompletions lineStr caret getCompletions with
    | None -> false
    | Some (name, replacementIndex, ident, completions) ->

    let completions = fixComplete name completions ident

    completeLine line replacementIndex (caret - replacementIndex) completions
    editor.Redraw()
    true
