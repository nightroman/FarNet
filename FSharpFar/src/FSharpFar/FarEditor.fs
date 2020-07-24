namespace FSharpFar
open FarNet
open FarNet.FSharp
open FSharp.Compiler.SourceCodeServices
open System

type LineArgs = {
    Text : string
    Index : int
    Column : int
}

type MouseMessage =
| Noop
| Move of LineArgs

[<ModuleEditor (Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
[<Guid "B7916B53-2C17-4086-8F13-5FFCF0D82900">]
type FarEditor () =
    inherit ModuleEditor ()
    let mutable editor: IEditor = null

    let jobEditor f = Job.FromContinuations (fun (cont, econt, ccont) ->
        if editor.IsOpened then
            cont (f ())
        else
            ccont (OperationCanceledException ())
    )

    let checkAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            do! inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            do! Async.Sleep 1000
            if inbox.CurrentQueueLength > 0 then () else

            let! text = jobEditor (fun () -> Editor.sourceText editor)
            try
                let config = editor.MyConfig ()
                let! check = Checker.check editor.FileName text config
                editor.MyErrors <-
                    if inbox.CurrentQueueLength > 0 then
                        None
                    else
                        let errors = check.CheckResults.Errors
                        if errors.Length = 0 then None else Some errors
                do! jobEditor editor.Redraw
            with exn ->
                Job.PostShowError exn
    })

    let mouseAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            let! message = inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            match message with
            | Noop -> ()
            | Move it ->
                do! Async.Sleep 400
                if inbox.CurrentQueueLength > 0 then () else

                let mutable autoTips = editor.MyAutoTips
                match editor.MyFileErrors () with
                | None -> ()
                | Some errors ->
                    let lines =
                        errors
                        |> Array.filter (fun err ->
                            it.Index >= err.StartLineAlternate - 1 &&
                            it.Index <= err.EndLineAlternate - 1 &&
                            (it.Index > err.StartLineAlternate - 1 || it.Column >= err.StartColumn) &&
                            (it.Index < err.EndLineAlternate - 1 || it.Column <= err.EndColumn))
                        |> Array.map FSharpErrorInfo.strErrorText
                        |> Array.distinct
                    if lines.Length > 0 then
                        autoTips <- false
                        let text = String.Join ("\r", lines)
                        do! jobEditor (fun _ -> showText text "Errors")

                if autoTips then
                    match Parser.findLongIdents it.Column it.Text with
                    | None -> ()
                    | Some (column, idents) ->
                        let! text = jobEditor (fun () -> Editor.sourceText editor)
                        try
                            let config = editor.MyConfig ()
                            let! check = Checker.check editor.FileName text config
                            let! tip = check.CheckResults.GetToolTipText (it.Index + 1, column + 1, it.Text, idents, FSharpTokenTag.Identifier)
                            let tips = Tips.format tip false
                            if tips.Length > 0 && inbox.CurrentQueueLength = 0 then
                                do! jobEditor (fun _ ->
                                    let r = far.Message (tips, "Tips", MessageOptions.LeftAligned, [|"More"; "Close"|])
                                    if r = 0 then
                                        showTempText (Tips.format tip true) (String.Join (".", List.toArray idents))
                                )
                        with exn ->
                            Job.PostShowError exn
    })

    let postNoop _ =  mouseAgent.Post Noop

    override __.Invoke (sender, _) =
        editor <- sender
        if editor.MySession.IsNone then

            editor.Saving.Add <| fun e ->
                Session.OnSavingSource e.FileName

            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is () && not editor.SelectionExists ->
                     e.Ignore <- Editor.complete editor
                | KeyCode.F5 when e.Key.Is () ->
                     Editor.load editor
                     e.Ignore <- true
                | _ -> ()

            editor.Changed.Add <| fun e ->
                let isAutoCheck = editor.MyAutoCheck

                // We want to keep errors visible, so that on typing fixes we see how errors go.
                // This does not work well on massive changes like copy/paste, delete lines.
                // So keep errors if just a line changes to draw them in "checking" mode.
                if e.Kind = EditorChangeKind.LineChanged && isAutoCheck then
                    editor.MyChecking <- true
                else
                    editor.MyErrors <- None

                if isAutoCheck then
                    checkAgent.Post ()

            editor.MouseDoubleClick.Add postNoop
            editor.MouseClick.Add postNoop
            editor.MouseWheel.Add postNoop

            editor.MouseMove.Add <| fun e ->
                mouseAgent.Post (
                    if e.Mouse.Is () then
                        let pos = editor.ConvertPointScreenToEditor e.Mouse.Where
                        if pos.Y < editor.Count then
                            let line = editor.[pos.Y]
                            if pos.X < line.Length then
                                Move {Text = line.Text; Index = pos.Y; Column = pos.X}
                            else Noop
                        else Noop
                    else Noop
                )
