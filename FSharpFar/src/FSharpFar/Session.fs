namespace FSharpFar
open System
open System.IO
open System.Diagnostics
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Diagnostics

[<NoComparison>]
type EvalResult = {
    Warnings: FSharpDiagnostic []
    Exception: exn
}

[<RequireQualifiedAccess>]
module FSharpDiagnostic =
    let private strErrorSeverity = function
        | FSharpDiagnosticSeverity.Error -> "error"
        | FSharpDiagnosticSeverity.Warning -> "warning"
        | FSharpDiagnosticSeverity.Info -> "info"
        | FSharpDiagnosticSeverity.Hidden -> "hidden"

    /// Error text as it is without source info.
    let strErrorText (x : FSharpDiagnostic) =
        sprintf "%s FS%04d: %s" (strErrorSeverity x.Severity) x.ErrorNumber x.Message

    /// Error text as it is with full source info.
    let strErrorFull (x : FSharpDiagnostic) =
        $"{x.FileName}({x.StartLine},{x.StartColumn + 1}): {strErrorText x}"

    /// Error text as line with mini source info.
    let strErrorLine (x : FSharpDiagnostic) =
        $"{Path.GetFileName x.FileName}({x.StartLine},{x.StartColumn + 1}): {strAsLine (strErrorText x)}"

type Session private (configFile) =
    static let mutable sessions : Session list = []
    let onClose = Event<unit> ()

    // The writer for some extra "noise" output, like loading assemblies.
    // It is also useful and used for capturing config file problems.
    let hiddenWriter = new StringWriter()

    // The permanent writer attached to the session, as required by FCS.
    // The actual writer is changing inside it depending on eval.
    let evalWriter = new ProxyWriter(hiddenWriter)

    let fsiSession, errors, ok, config =
        use _progress = new Progress "Loading session..."
        let mutable ok = true

        let config = Config.readFromFile configFile
        let args = [|
            yield "fsi.exe" //! dummy ~ fsi.CommandLineArgs[0]
            yield "--nologo"
            yield "--noninteractive"
            yield! defaultCompilerArgs
            yield! config.FscArgs
            yield! config.FsiArgs
            //! do not worry about duplicates (or deal with: --optimize- --optimize+ ~ both must be removed)
            if Debugger.IsAttached then
                yield "--optimize-"
                yield "--debug:full"
                yield "--define:DEBUG"
        |]

        //! collectible=true has issues
        let fsiSession =
            try
                let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
                FsiEvaluationSession.Create(fsiConfig, args, stdin, evalWriter, evalWriter)
            with exn ->
                // case: unknown option in [fsc]
                raise (InvalidOperationException("Cannot create a session. Ensure valid configuration syntax and data." + hiddenWriter.ToString(), exn))

        // load and use files

        use writer = new StringWriter()

        let check (result, warnings) =
            for w in warnings do
                writer.WriteLine(FSharpDiagnostic.strErrorFull w)

            match result with
            | Choice2Of2 exn ->
                Exn.reraise exn
            | _ ->
                ()

        try
            for file in Array.append config.FscFiles config.FsiFiles do
                fsiSession.EvalScriptNonThrowing file
                |> check

            for file in config.UseFiles do
                let code = File.ReadAllText file
                fsiSession.EvalInteractionNonThrowing code
                |> check

        with exn ->
            ok <- false
            fprintfn writer "%A" exn

        fsiSession, writer.ToString(), ok, config

    // Calls the evaluation with the custom writer.
    // writer: The writer used during evaluation.
    // f: Evaluates the code or file.
    let eval writer f =
        try
            evalWriter.Writer <- writer
            let result, warnings = f ()
            {
                Warnings = warnings
                Exception = match result with Choice2Of2 exn -> exn | _ -> null
            }
        finally
            // do not leave the temp writer attached, fsi still writes, e.g. about loaded assemblies
            evalWriter.Writer <- hiddenWriter

    // Used by GetOrCreate and by TryDefaultSession.
    static member private TryFind path =
        sessions |> List.tryFind (fun x -> String.equalsIgnoreCase x.ConfigFile path)

    /// Gets the existing or creates a new session specified by the config path.
    /// If the path is a directory then its config is used if any else it fails.
    /// path: The full path of config file (normalized, case insensitive).
    static member GetOrCreate path =
        assert (path = Path.GetFullPath path)
        let path =
            if File.Exists path then
                path
            elif Directory.Exists path then
                match Config.tryFindFileInDirectory path with
                | Some path ->
                    path
                | None ->
                    invalidOp $"Cannot find the config file in '{path}'."
            else
                invalidOp $"Cannot find the config file or directory '{path}'."

        match Session.TryFind path with
        | Some ses ->
            ses
        | None ->
            let ses = Session path
            sessions <- ses :: sessions
            ses

    /// Close affected sessions on saving configs.
    static member OnSavingConfig path =
        assert (path = Path.GetFullPath path)
        for ses in sessions do
            if String.equalsIgnoreCase ses.ConfigFile path then
                ses.Close()

    /// Close affected sessions on saving sources.
    static member OnSavingSource path =
        assert (path = Path.GetFullPath path)
        for ses in sessions do
            let config = ses.Config
            if
                Seq.containsIgnoreCase path config.FscFiles ||
                Seq.containsIgnoreCase path config.FsiFiles ||
                Seq.containsIgnoreCase path config.UseFiles
                then
                    ses.Close()

    /// Gets or creates the root session.
    static member DefaultSession() =
        Session.GetOrCreate(Config.defaultFile ())

    /// Gets the main session or none.
    static member TryDefaultSession() =
        Session.TryFind(Config.defaultFile ())

    /// Gets the list of created sessions.
    static member Sessions = sessions

    /// Closes the session and resources and triggers OnClose.
    member x.Close() =
        onClose.Trigger()

        sessions <- sessions |> List.except [x]

        evalWriter.Dispose()
        hiddenWriter.Dispose()
        (fsiSession :> IDisposable).Dispose()

    /// The full path of config file (normalized, case insensitive).
    member val ConfigFile = configFile

    /// The session configuration.
    member val Config = config

    /// The session display name for menus, editors, etc.
    member val DisplayName = $"{Path.GetFileName configFile} - {Path.GetDirectoryName configFile}"

    /// The session loading errors.
    member val Errors = errors

    /// The session is OK.
    member val Ok = ok

    /// Called on closing.
    member val OnClose = onClose.Publish

    member __.EvalInteraction(writer, code) =
        eval writer (fun () -> fsiSession.EvalInteractionNonThrowing code)

    member __.EvalScript(writer, filePath) =
        eval writer (fun () -> fsiSession.EvalScriptNonThrowing filePath)

    member __.GetCompletions(longIdent) =
        try
            fsiSession.GetCompletions longIdent
        with _ ->
            //! SplitPipeline.SplitPipelineCommand. -> exn
            Seq.empty

    static member Eval(writer, eval : unit -> EvalResult) =
        let oldOut = Console.Out
        let oldError = Console.Error
        let r =
            try
                Console.SetOut writer
                Console.SetError writer
                eval ()
            finally
                Console.SetOut oldOut
                Console.SetError oldError
        for w in r.Warnings do
            writer.WriteLine(FSharpDiagnostic.strErrorFull w)
        if not (isNull r.Exception) then
            fprintfn writer "%A" r.Exception
