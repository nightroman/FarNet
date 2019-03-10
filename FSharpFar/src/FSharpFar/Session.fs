namespace FSharpFar
open System
open System.IO
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.SourceCodeServices

[<NoComparison>]
type EvalResult = {
    Warnings: FSharpErrorInfo []
    Exception: exn
}

[<RequireQualifiedAccess>]
module FSharpErrorInfo =
    let private strErrorSeverity = function
        | FSharpErrorSeverity.Error -> "error"
        | FSharpErrorSeverity.Warning -> "warning"

    /// Error text as it is without source info.
    let strErrorText (x : FSharpErrorInfo) =
        sprintf "%s FS%04d: %s" (strErrorSeverity x.Severity) x.ErrorNumber x.Message

    /// Error text as it is with full source info.
    let strErrorFull (x : FSharpErrorInfo) =
        sprintf "%s(%d,%d): %s" x.FileName x.StartLineAlternate (x.StartColumn + 1) (strErrorText x)

    /// Error text as line with mini source info.
    let strErrorLine (x : FSharpErrorInfo) =
        sprintf "%s(%d,%d): %s" (Path.GetFileName x.FileName) x.StartLineAlternate (x.StartColumn + 1) (strAsLine (strErrorText x))

type Session private (configFile) =
    static let mutable sessions : Session list = []
    let onClose = Event<unit> ()

    // The writer for some extra "noise" output, like loading assemblies.
    let voidWriter = new StringWriter ()

    // The permanent writer attached to the session, as required by FCS.
    // The actual writer is changing inside it depending on eval.
    let evalWriter = new ProxyWriter (voidWriter)

    let fsiSession, errors, config =
        use _progress = new Progress "Loading session..."

        let config = Config.readFromFile configFile
        let args = [|
            yield "fsi.exe" //! dummy, else --nologo is consumed
            yield "--nologo"
            yield "--noninteractive"
            yield! defaultCompilerArgs
            yield! config.FscArgs
            yield! config.FsiArgs
        |]
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration ()

        //! collectible=true has issues
        let fsiSession =
            try
                FsiEvaluationSession.Create (fsiConfig, args, new StringReader "", evalWriter, evalWriter)
            with exn ->
                // case: `//--define:DEBUG` in [fsc]
                raise (InvalidOperationException ("Cannot create a session. If you use a config file check its syntax and data.", exn))

        // load and use files
        use writer = new StringWriter ()
        try
            for file in config.FscFiles @ config.FsiFiles do
                let result, warnings = fsiSession.EvalInteractionNonThrowing (sprintf "#load @\"%s\"" file)
                for w in warnings do writer.WriteLine (FSharpErrorInfo.strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()

            for file in config.UseFiles do
                let code = File.ReadAllText file
                let result, warnings = fsiSession.EvalInteractionNonThrowing code
                for w in warnings do writer.WriteLine (FSharpErrorInfo.strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()
        with exn ->
            fprintfn writer "%A" exn

        fsiSession, writer.ToString (), config

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
            evalWriter.Writer <- voidWriter

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
                    invalidOp <| sprintf "Cannot find the config file in '%s'." path
            else
                invalidOp <| sprintf "Cannot find the config file or directory '%s'." path

        match Session.TryFind path with
        | Some ses ->
            ses
        | None ->
            let ses = Session path
            sessions <- ses :: sessions
            ses

    /// Gets or creates the root session.
    static member DefaultSession () =
        Session.GetOrCreate (Config.defaultFile ())

    /// Gets the main session or none.
    static member TryDefaultSession () =
        Session.TryFind (Config.defaultFile ())

    /// Gets the list of created sessions.
    static member Sessions = sessions

    /// Closes the session and resources and triggers OnClose.
    member x.Close () =
        onClose.Trigger ()

        sessions <- sessions |> List.except [x]

        evalWriter.Dispose ()
        voidWriter.Dispose ()
        (fsiSession :> IDisposable).Dispose ()

    /// The full path of config file (normalized, case insensitive).
    member val ConfigFile = configFile

    /// The session configuration.
    member val Config = config

    /// The session display name for menus, editors, etc.
    member val DisplayName = sprintf "%s - %s" (Path.GetFileName configFile) (Path.GetDirectoryName configFile)

    /// The session loading errors.
    member val Errors = errors

    /// Called on closing.
    member val OnClose = onClose.Publish

    member __.EvalInteraction (writer, code) =
        eval writer (fun () -> fsiSession.EvalInteractionNonThrowing code)

    member __.EvalScript (writer, filePath) =
        eval writer (fun () -> fsiSession.EvalScriptNonThrowing filePath)

    member __.GetCompletions (longIdent) =
        try
            fsiSession.GetCompletions longIdent
        with _ ->
            //! SplitPipeline.SplitPipelineCommand. -> exn
            Seq.empty

    static member Eval (writer, eval : unit -> EvalResult) =
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
            writer.WriteLine (FSharpErrorInfo.strErrorFull w)
        if not (isNull r.Exception) then
            fprintfn writer "%A" r.Exception
