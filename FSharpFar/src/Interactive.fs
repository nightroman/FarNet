
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Interactive

open FarNet
open Command
open Session
open System
open System.IO
open System.Text

type Area =
    {
        FirstLineIndex : int
        LastLineIndex : int
        Caret : Point
        Active : bool
    }

let OutputMark1 = "(*("
let OutputMark2 = ")*)";

type Interactive(session : Session) =
    let _session = session
    let _editor = far.CreateEditor()

    let getCommandArea() =
        let caret = _editor.Caret

        let mutable line1 = caret.Y
        let rec findLine1 y =
            if y < 0 then ()
            else
                let text = _editor.[y].Text
                if text = OutputMark2 then
                    ()
                elif text = OutputMark1 then
                    line1 <- -1
                else
                    if text.Length > 0 then line1 <- y
                    findLine1 (y - 1)

        findLine1 line1
        if line1 < 0 then
            None
        else
            let lineCount = _editor.Count
            let mutable active = true
            let mutable line2 = line1

            let rec findLine2 y =
                if y >= lineCount then ()
                else
                    let text = _editor.[y].Text
                    if text = OutputMark1 then
                        active <- false
                    elif text = OutputMark2 then
                        line2 <- -1
                    else
                        if text.Length > 0 then line2 <- y
                        findLine2 (y + 1)

            findLine2 line2
            if line2 < 0 then None
            else Some {
                FirstLineIndex = line1
                LastLineIndex = line2
                Caret = caret
                Active = active
            }

    let areaCode area =
        let sb = StringBuilder()
        for y in area.FirstLineIndex .. area.LastLineIndex do
            if sb.Length > 0 then
                sb.AppendLine() |> ignore
            sb.Append _editor.[y].Text |> ignore
        sb.ToString()

    let complete() =
        let line = _editor.Line
        let caret = line.Caret
        if caret = 0 || caret > line.Length then false else

        let text = line.Text
        let completer = Completer.Completer(_session.GetCompletions)
        let ok, start, completions = completer.GetCompletions(text, caret)
        if ok then
            completeLine line start (caret - start) completions
            _editor.Redraw()
        ok

    let invoke() =
        let area = getCommandArea()
        match area with
        | None -> ()
        | Some area ->
            let code = areaCode area
            if code.Length = 0 then
                ()
            elif not area.Active then
                _editor.GoToEnd true
                _editor.BeginUndo()
                _editor.InsertText code
                _editor.EndUndo()
                _editor.Redraw()
            else
                match parseCommand code with
                | Quit ->
                    _session.Close()
                | _ ->

                _editor.BeginUndo()
                _editor.GoToEnd false
                if not (String.IsNullOrWhiteSpace(_editor.Line.Text)) then
                    _editor.InsertLine()
                _editor.InsertText (sprintf "\r%s\r" OutputMark1)

                let writer = _editor.OpenWriter()
                let oldOut = Console.Out
                let oldErr = Console.Error
                let r =
                    try
                        Console.SetOut writer
                        Console.SetError writer
                        _session.EvalInteraction(writer, code)
                    finally
                        Console.SetOut oldOut
                        Console.SetError oldOut

                for w in r.Warnings do
                    writer.WriteLine(formatFSharpErrorInfo w)
                if r.Exception <> null then
                    writer.WriteLine(sprintf "%A" r.Exception)

                writer.WriteLine OutputMark2
                writer.WriteLine()

                _editor.EndUndo()
                _editor.Redraw()

    static let GuidColor = Guid("8fb3dd25-b0a9-4940-a5fe-67621c47250d")
    let draw() =
        let size = _editor.WindowSize
        let frame = _editor.Frame
        let lineCount = _editor.Count

        let startLine = frame.VisibleLine
        let endLine = min (startLine + size.Y) lineCount

        let startChar = frame.VisibleChar
        let endChar = frame.VisibleChar + size.X

        let colors = seq {
            for y in startLine .. endLine - 1 do
                let text = _editor.[y].Text
                if text = OutputMark1 || text = OutputMark2 then
                    yield EditorColor(y, startChar, endChar, ConsoleColor.White, ConsoleColor.White)
            }

        _editor.WorksSetColors(GuidColor, 2, colors)

    member __.Open() =
        let path = _session.EditorFile

        _editor.FileName <- path
        _editor.CodePage <- 65001
        _editor.Title <- sprintf "F# %s - %s" (Path.GetFileName path) (Path.GetDirectoryName path)

        _editor.KeyDown.Add(fun e ->
            if not _editor.SelectionExists then
                match e.Key.VirtualKeyCode with
                | KeyCode.Enter ->
                    if e.Key.IsShift() then
                        e.Ignore <- true
                        invoke()
                | KeyCode.Tab ->
                    if e.Key.Is() then
                        e.Ignore <- complete()
                | _ -> ()
            )

        _editor.Open()

        // attach to session
        _session.OnClose <- fun() ->
            if _editor.IsOpened then
                _editor.Close()

        if _session.Issues.Length > 0 then
            showText "F# Issues" _session.Issues
