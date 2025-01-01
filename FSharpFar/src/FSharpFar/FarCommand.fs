namespace FSharpFar
open FarNet
open System
open System.IO
open System.Diagnostics
open FarInteractive
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Interactive.Shell
open System.Runtime.ExceptionServices

[<ModuleCommand(Name = "FSharpFar", Prefix = "fs", Id = "2b52615b-ea79-46e4-ac9d-78f33599db62")>]
type FarCommand() =
    inherit ModuleCommand()

    let echo (command: string) =
        far.UI.WriteLine($"fs:{command}", ConsoleColor.DarkGray)

    let writeResult r =
        // first write warnings and errors
        for w in r.Warnings do
            let color = match w.Severity with FSharpDiagnosticSeverity.Error -> ConsoleColor.Red | _ -> ConsoleColor.Yellow
            let message = FSharpDiagnostic.strErrorFull w
            far.UI.WriteLine(message, color)

            //! 2024-12-30-0920 fail on testing or it will formally pass
            if FarNet.Works.Test.IsTestCommand && isNull r.Exception then
                failwith message

        // then throw an exception (do not write or the caller has no way to know and handle it)
        // exceptions may be due to compile errors (written above) and runtime exceptions
        if not (isNull r.Exception) then
            //! preserve stack trace of runtime exceptions
            (ExceptionDispatchInfo.Capture r.Exception).Throw()

    let commandCode code =
        let ses = Session.DefaultSession()
        if ses.Errors.Length > 0 then
            far.UI.Write(ses.Errors, ConsoleColor.Red)

        if ses.Ok then
            use writer = new StringWriter()
            let r = ses.EvalInteraction(writer, code)

            far.UI.Write(writer.ToString())
            writeResult r

    let commandCompile (args: Command.CompileArgs) =
        let configPath = Config.ensureConfigPath args.With
        let config = Config.readFromFile configPath

        let errors, _ = Checker.compile config configPath |> Async.RunSynchronously
        if errors.Length > 0 then
            use writer = new StringWriter()
            for error in errors do
                writer.WriteLine(FSharpDiagnostic.strErrorLine error)
            showTempText (writer.ToString()) "Errors"

    let commandQuit () =
        match Session.TryDefaultSession() with
        | Some ses -> ses.Close()
        | None -> far.UI.WriteLine "The session is not opened."

    let commandOpen (args: Command.OpenArgs) =
        let ses =
            match args.With with
            | Some path ->
                Session.GetOrCreate path
            | None ->
                Session.DefaultSession()
        FarInteractive(ses).Open()

    let commandProject (args: Command.ProjectArgs) =
        let configPath = Config.ensureConfigPath args.With
        let directoryPath = Path.GetDirectoryName configPath
        Watcher.add directoryPath

        let projectPath = Config.generateProject configPath args.Type

        match args.Open with
        | Command.ProjectOpenBy.VS ->
            ProcessStartInfo(projectPath, UseShellExecute = true)
            |> Process.Start
            |> ignore
        | Command.ProjectOpenBy.VSCode ->
            let dir = Path.GetDirectoryName projectPath
            Config.writeVSCodeSettings dir
            try
                ProcessStartInfo("code.cmd", "\"" + dir + "\"", UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden)
                |> Process.Start
                |> ignore
            with exn ->
                showText exn.Message "Cannot start code.cmd"

    let commandExec command (args: Command.ExecArgs) =
        //! fs:exec ;; TryPanelFSharp.run () // must pick up the root config
        let ses =
            match args.With, args.File with
            | Some configPath, _ -> configPath
            | _, Some filePath -> Config.defaultFileForFile filePath
            | _ -> Config.defaultFile ()
            |> Session.GetOrCreate

        let echo =
            (lazy (echo command)).Force

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

    override _.Invoke(_, e) =
        let command = e.Command
        try
            match Command.parse (command.AsSpan()) with

            | Command.Code code ->
                use _ = new FarNet.FSharp.Works.FarStdWriter()
                echo command
                commandCode code

            | Command.Compile args ->
                use _ = new Progress "Compiling..."
                commandCompile args

            | Command.Exec args ->
                use _ = new FarNet.FSharp.Works.FarStdWriter()
                commandExec command args

            | Command.Open args ->
                commandOpen args

            | Command.Project args ->
                commandProject args

            | Command.Quit ->
                commandQuit ()

        with
        | :? FsiCompilationException as ex ->
            raise (ModuleException(ex.Message, Source = "F# compiler"))
        | ex ->
            raise (ModuleException(ex.Message, ex))
