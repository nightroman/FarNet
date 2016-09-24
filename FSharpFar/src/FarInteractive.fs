
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FarInteractive

open FarNet
open Command
open Session
open System
open System.IO
open System.Text

type private Area = {
    HeadLineIndex : int
    LastLineIndex : int
    Caret : Point
    Active : bool
}

let private OutputMark1 = "(*("
let private OutputMark2 = ")*)";

type FarInteractive(session : Session) =
    let session = session
    let editor = far.CreateEditor()

    let getCommandArea() =
        let caret = editor.Caret

        let mutable line1 = caret.Y
        let rec findLine1 y =
            if y < 0 then ()
            else
                let text = editor.[y].Text
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
            let lineCount = editor.Count
            let mutable active = true
            let mutable line2 = line1

            let rec findLine2 y =
                if y >= lineCount then ()
                else
                    let text = editor.[y].Text
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
                HeadLineIndex = line1
                LastLineIndex = line2
                Caret = caret
                Active = active
            }

    let areaCode area =
        let sb = StringBuilder()
        for y in area.HeadLineIndex .. area.LastLineIndex do
            if sb.Length > 0 then
                sb.AppendLine() |> ignore
            sb.Append editor.[y].Text |> ignore
        sb.ToString()

    let invoke() =
        match getCommandArea() with
        | None -> ()
        | Some area ->
        
        let code = areaCode area
        if code.Length = 0 then
            ()
        
        elif not area.Active then
            editor.GoToEnd true
            editor.BeginUndo()
            editor.InsertText code
            editor.EndUndo()
            editor.Redraw()
 
        // one line with a command, now just #quit, ignore others       
        elif area.HeadLineIndex = area.LastLineIndex && (match parseCommand code with Quit -> true | _ -> false) then
            session.Close()
        
        else
                
        editor.BeginUndo()
        editor.GoToEnd false
        if not (String.IsNullOrWhiteSpace(editor.Line.Text)) then
            editor.InsertLine()
        editor.InsertText (sprintf "\r%s\r" OutputMark1)

        let writer = editor.OpenWriter()

        doEval writer (fun () -> session.EvalInteraction(writer, code))

        writer.WriteLine OutputMark2
        writer.WriteLine()

        editor.EndUndo()
        editor.Redraw()

    static let GuidColor = Guid("8fb3dd25-b0a9-4940-a5fe-67621c47250d")
    let draw() =
        let size = editor.WindowSize
        let frame = editor.Frame
        let lineCount = editor.Count

        let startLine = frame.VisibleLine
        let endLine = min (startLine + size.Y) lineCount

        let startChar = frame.VisibleChar
        let endChar = frame.VisibleChar + size.X

        let colors = seq {
            for y in startLine .. endLine - 1 do
                let text = editor.[y].Text
                if text = OutputMark1 || text = OutputMark2 then
                    yield EditorColor(y, startChar, endChar, ConsoleColor.White, ConsoleColor.White)
            }

        editor.WorksSetColors(GuidColor, 2, colors)

    member x.Open() =
        let path = session.EditorFile

        editor.FileName <- path
        editor.CodePage <- 65001
        editor.Title <- sprintf "F# Interactive %s" session.DisplayName
        
        // attach to session
        editor.fsSession <- Some session
        let onSessionClose = Handler<unit>(fun _ _ ->
            if editor.IsOpened then
                editor.Close()
        )
        session.OnClose.AddHandler onSessionClose
        editor.Closed.Add(fun _ ->
            session.OnClose.RemoveHandler onSessionClose
        )

        editor.KeyDown.Add <| fun e ->
            if not editor.SelectionExists then
                match e.Key.VirtualKeyCode with
                | KeyCode.Enter when e.Key.IsShift() ->
                    e.Ignore <- true
                    invoke()
                | KeyCode.Tab when e.Key.Is() && not editor.SelectionExists ->
                    e.Ignore <- completeCode editor session.GetCompletions
                | _ ->
                    ()

        // Open. Post, to avoid modal. Use case:
        // - open session by `fs: //open`
        // - it writes echo -> user screen
        // - opening from user screen is modal
        far.PostJob(fun() ->
            editor.Open()
        )

        // Show issues. Post, for legit modal cases like opening from a dialog.
        // We want some job to be done after opening in any case.
        far.PostJob(fun() ->
            if session.Issues.Length > 0 then
                showTempText session.Issues "F# Issues"
        )
