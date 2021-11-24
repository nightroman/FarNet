namespace FSharpFar
open FarNet
open System
open System.IO
open FarInteractive

[<ModuleCommand(Name = "FSharpFar", Prefix = "fs")>]
[<Guid "2b52615b-ea79-46e4-ac9d-78f33599db62">]
type FarCommand() =
    inherit ModuleCommand()

    let invoke command =
        let echo () =
            far.UI.WriteLine($"fs:{command}", ConsoleColor.DarkGray)

        let writeResult r =
            for w in r.Warnings do
                far.UI.WriteLine(FSharpDiagnostic.strErrorFull w, ConsoleColor.Yellow)
            if not (isNull r.Exception) then
                writeException r.Exception

        match Command.parse command with
        | Command.Quit ->
            match Session.TryDefaultSession() with
            | Some ses -> ses.Close()
            | None -> far.UI.WriteLine "The session is not opened."

        | Command.Open args ->
            let ses =
                match args.With with
                | Some path -> Session.GetOrCreate path
                | _ -> Session.DefaultSession()
            FarInteractive(ses).Open()

        | Command.Code code ->
            echo ()
            use _std = new FarNet.FSharp.Works.FarStdWriter()

            let ses = Session.DefaultSession()
            if ses.Errors.Length > 0 then
                far.UI.Write(ses.Errors, ConsoleColor.Red)

            if ses.Ok then
                use writer = new StringWriter()
                let r = ses.EvalInteraction(writer, code)

                far.UI.Write(writer.ToString())
                writeResult r

        | Command.Exec args ->
            try
                use _std = new FarNet.FSharp.Works.FarStdWriter()

                //! fs: //exec ;; TryPanelFSharp.run () // must pick up the root config
                let ses =
                    match args.With, args.File with
                    | Some configPath, _ -> configPath
                    | _, Some filePath -> Config.defaultFileForFile filePath
                    | _ -> Config.defaultFile ()
                    |> Session.GetOrCreate

                let echo =
                    (lazy (echo ())).Force

                use writer = new StringWriter()
                let validate r =
                    if r.Warnings.Length > 0 || not (isNull r.Exception) then
                        echo ()
                        far.UI.Write(writer.ToString())
                        writeResult r
                        false
                    else
                        true

                // session errors first or issues may look cryptic
                if ses.Errors.Length > 0 then
                    echo ()
                    far.UI.Write ses.Errors

                // eval anyway, session errors may be warnings
                let ok =
                    if ses.Ok then
                        match args.File with
                        | Some file ->
                            let r = ses.EvalScript(writer, file)
                            validate r
                        | None ->
                            true
                    else
                        false

                match ok, args.Code with
                | true, Some code ->
                    let r = ses.EvalInteraction(writer, code)
                    validate r |> ignore
                | _ ->
                    ()

            with exn ->
                // e.g. on missing file
                far.ShowError("fs: //exec", exn)

        | Command.Compile args ->
            use _progress = new Progress "Compiling..."

            let path =
                match args.With with
                | Some path ->
                    path
                | None ->
                    match Config.tryFindFileInDirectory far.CurrentDirectory with
                    | Some path ->
                        path
                    | None ->
                        invalidOp "Cannot find configuration file."

            let config = Config.readFromFile path

            let errors, code = Checker.compile config |> Async.RunSynchronously
            if errors.Length > 0 then
                use writer = new StringWriter()
                for error in errors do
                    writer.WriteLine(FSharpDiagnostic.strErrorLine error)
                showTempText (writer.ToString()) "Errors"
            ()

    override __.Invoke(_, e) =
        try
            invoke e.Command
        with
            Failure error ->
                raise (ModuleException(error, Source = "F# command"))
