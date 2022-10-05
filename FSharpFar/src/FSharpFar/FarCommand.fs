namespace FSharpFar
open FarNet
open System
open System.IO
open System.Diagnostics
open FarInteractive

[<ModuleCommand(Name = "FSharpFar", Prefix = "fs", Id = "2b52615b-ea79-46e4-ac9d-78f33599db62")>]
type FarCommand() =
    inherit ModuleCommand()

    let echo (command: string) =
        far.UI.WriteLine($"fs:{command}", ConsoleColor.DarkGray)

    let writeResult r =
        for w in r.Warnings do
            far.UI.WriteLine(FSharpDiagnostic.strErrorFull w, ConsoleColor.Yellow)
        if not (isNull r.Exception) then
            writeException r.Exception

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
        //! fs: exec: ;; TryPanelFSharp.run () // must pick up the root config
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

    let invoke command =
        match Command.parse command with

        | Command.Code code ->
            use _ = new FarNet.FSharp.Works.FarStdWriter()
            echo command
            commandCode code

        | Command.Compile args ->
            use _ = new Progress "Compiling..."
            commandCompile args

        | Command.Exec args ->
            try
                use _ = new FarNet.FSharp.Works.FarStdWriter()
                commandExec command args
            with exn ->
                // e.g. on missing file
                far.ShowError("fs: exec:", exn)

        | Command.Open args ->
            commandOpen args

        | Command.Project args ->
            commandProject args

        | Command.Quit ->
            commandQuit ()

    override _.Invoke(_, e) =
        try
            invoke e.Command

        with Failure error ->
            raise (ModuleException(error, Source = "F# command"))
