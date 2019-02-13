module FSharpFar.Session
open System
open System.IO
open Config
open ProxyWriter
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices

[<NoComparison>]
type EvalResult = {
    Warnings: FSharpErrorInfo []
    Exception: exn
}

let strErrorSeverity = function
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

let doEval writer (eval : unit -> EvalResult) =
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
        writer.WriteLine (strErrorFull w)
    if not (isNull r.Exception) then
        fprintfn writer "%A" r.Exception

type Session private (configFile) =
    static let mutable sessions : Session list = []
    let configFile = Path.GetFullPath configFile
    let onClose = new Event<unit> ()

    // contains some extra "noise" output
    let voidWriter = new StringWriter ()
    // assigned to the session
    let evalWriter = new ProxyWriter (voidWriter)

    let fsiSession, errors, config =
        use _progress = new Progress "Loading session..."

        let config = if File.Exists configFile then readConfigFromFile configFile else Config.empty
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
                for w in warnings do writer.WriteLine (strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()

            for file in config.UseFiles do
                let code = File.ReadAllText file
                let result, warnings = fsiSession.EvalInteractionNonThrowing code
                for w in warnings do writer.WriteLine (strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()
        with exn ->
            fprintfn writer "%A" exn

        fsiSession, writer.ToString (), config

    let eval writer eval x =
        try
            evalWriter.Writer <- writer
            let result, warnings = eval x
            {
                Warnings = warnings
                Exception = match result with Choice2Of2 exn -> exn | _ -> null
            }
        finally
            // do not leave the temp writer attached, fsi still writes, e.g. about loaded assemblies
            evalWriter.Writer <- voidWriter

    static member TryFind (path) =
        sessions |> List.tryFind (fun x -> x.IsSameConfigFile path)

    /// Gets the existing or creates a new session specified by the config path.
    /// If the path is a directory then its config is used if any else it fails.
    static member GetOrCreate path =
        let path =
            if File.Exists path then
                path
            elif Directory.Exists path then
                match tryConfigPathInDirectory path with
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
            sessions <- Session path :: sessions
            sessions.Head

    static member Sessions = sessions

    member x.Close () =
        onClose.Trigger ()

        sessions <- sessions |> List.except [x]

        evalWriter.Dispose ()
        voidWriter.Dispose ()
        (fsiSession :> IDisposable).Dispose ()

    member val ConfigFile = configFile

    member val Config = config

    member val DisplayName = sprintf "%s - %s" (Path.GetFileName configFile) (Path.GetDirectoryName configFile)

    member val Errors = errors

    [<CLIEvent>]
    member val OnClose = onClose.Publish

    member __.IsSameConfigFile path =
        String.equalsIgnoreCase configFile (Path.GetFullPath path)

    member __.EvalInteraction (writer, code) =
        eval writer fsiSession.EvalInteractionNonThrowing code

    member __.EvalScript (writer, filePath) =
        eval writer fsiSession.EvalScriptNonThrowing filePath

    member __.GetCompletions (longIdent) =
        try
            fsiSession.GetCompletions longIdent
        with _ ->
            //! SplitPipeline.SplitPipelineCommand. -> exn
            Seq.empty

/// Gets or creates the root session.
let getRootSession () = Session.GetOrCreate (getRootConfigPath ())

/// Gets the main session or none.
let tryRootSession () = Session.TryFind (getRootConfigPath ())
