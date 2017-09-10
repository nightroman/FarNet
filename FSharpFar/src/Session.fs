
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Session

open System
open System.IO
open Config
open ProxyWriter
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices

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
        use progress = new Progress "Loading session..."

        let config = if File.Exists configFile then getConfigFromFileCached configFile else Config.empty
        let loadFiles = ResizeArray ()
        let useFiles = ResizeArray ()
        let args = [|
            yield "fsi.exe" //! dummy, else --nologo is consumed
            yield "--nologo"
            yield "--noninteractive"
            yield! defaultCompilerArgs
            yield! config.FscArgs
            yield! config.FsiArgs
            loadFiles.AddRange config.LoadFiles
            useFiles.AddRange config.UseFiles
        |]
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration ()

        //! collectible=true has issues
        let fsiSession =
            try
                FsiEvaluationSession.Create (fsiConfig, args, new StringReader "", evalWriter, evalWriter)
            with exn ->
                // case: `//define=DEBUG` in [fsc]
                raise (InvalidOperationException ("Cannot create a session. If you use a config file check its syntax and data.", exn))

        // load and use files
        use writer = new StringWriter ()
        try
            for file in loadFiles do
                let result, warnings = fsiSession.EvalInteractionNonThrowing (sprintf "#load @\"%s\"" file)
                for w in warnings do writer.WriteLine (strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()

            for file in useFiles do
                let code = File.ReadAllText file
                let result, warnings = fsiSession.EvalInteractionNonThrowing code
                for w in warnings do writer.WriteLine (strErrorFull w)
                match result with Choice2Of2 exn -> raise exn | _ -> ()
        with exn ->
            fprintfn writer "%A" exn

        fsiSession, writer.ToString (), config

    let eval writer eval x =
        evalWriter.Writer <- writer
        let result, warnings = eval x
        //! do not leave the temp writer attached, fsi still writes, e.g. when PSF loads assemblies
        evalWriter.Writer <- voidWriter
        {
            Warnings = warnings
            Exception = match result with Choice2Of2 exn -> exn | _ -> null
        }

    static member TryFind (path) =
        sessions |> List.tryFind (fun x -> x.IsSameConfigFile path)

    static member FindOrCreate (path) =
        match Session.TryFind path with
        | Some s -> s
        | _ ->
            sessions <- Session path :: sessions
            sessions.Head

    static member Sessions = sessions

    member x.Close () =
        onClose.Trigger ()

        sessions <- sessions |> List.except [x]

        evalWriter.Dispose ()
        voidWriter.Dispose ()
        (fsiSession :> IDisposable).Dispose ()

    member x.ConfigFile = configFile

    member x.Config = config

    member x.DisplayName = sprintf "%s - %s" (Path.GetFileName configFile) (Path.GetDirectoryName configFile)

    member x.Errors = errors

    [<CLIEvent>]
    member x.OnClose = onClose.Publish

    member x.IsSameConfigFile path =
        String.Equals (configFile, Path.GetFullPath path, StringComparison.OrdinalIgnoreCase)

    member x.EvalInteraction (writer, code) =
        eval writer fsiSession.EvalInteractionNonThrowing code

    member x.EvalScript (writer, filePath) =
        eval writer fsiSession.EvalScriptNonThrowing filePath

    member x.GetCompletions (longIdent) =
        //! SplitPipeline.SplitPipelineCommand. -> exn
        try
            fsiSession.GetCompletions longIdent
        with _ ->
            Seq.empty

/// Gets or creates the main session.
let getMainSession () = Session.FindOrCreate farMainConfigPath

/// Gets the main session or none.
let tryFindMainSession () = Session.TryFind farMainConfigPath
