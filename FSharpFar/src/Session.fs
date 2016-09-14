
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Session

open ProxyWriter
open System
open System.IO
open System.Text
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Interactive.Shell

type EvalResult = {
    Warnings : FSharpErrorInfo[]
    Exception : exn
}

let strErrorSeverity(x) =
    match x with
    | FSharpErrorSeverity.Error -> "error"
    | FSharpErrorSeverity.Warning -> "warning"

let strErrorText(x : FSharpErrorInfo) =
    sprintf "%s(%d,%d): %s FS%04d: %s" x.FileName x.StartLineAlternate (x.StartColumn + 1) (strErrorSeverity x.Severity) x.ErrorNumber x.Message

let strErrorLine(x : FSharpErrorInfo) =
    sprintf "%s(%d,%d): %s FS%04d: %s" (Path.GetFileName x.FileName) x.StartLineAlternate (x.StartColumn + 1) (strErrorSeverity x.Severity) x.ErrorNumber (strLine x.Message)

let doEval writer (fn : unit -> EvalResult) =
    let oldOut = Console.Out
    let oldErr = Console.Error
    let r =
        try
            Console.SetOut writer
            Console.SetError writer
            fn()
        finally
            Console.SetOut oldOut
            Console.SetError oldOut
    for w in r.Warnings do
        writer.WriteLine(strErrorText w)
    if r.Exception <> null then
        writer.WriteLine(sprintf "%A" r.Exception)

let getCompilerOptions() =
    let dir = Path.Combine(Environment.GetEnvironmentVariable("FARHOME"), "FarNet")
    [|
        "--lib:" + dir
        "-r:" + dir + "\\FarNet.dll"
        "-r:" + dir + "\\FarNet.Tools.dll"
    |]

type Session private (from) =
    static let mutable sessions : Session list = []
    let from = Path.GetFullPath from

    // contains some extra "noise" output
    let _voidWriter = new StringWriter()
    // assigned to the session
    let _evalWriter = new ProxyWriter(_voidWriter)

    let fsiSession, issues, config =
        use progress = new UseProgress("Loading session...")

        let config = if File.Exists from then Config.getConfigurationFromFile from else Config.empty
        let args = [|
            yield "--nologo"
            yield "--noninteractive"
            yield! getCompilerOptions()
            yield! config.FscArgs
            yield! config.FsiArgs
        |]
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

        //! collectible=true has issues
        let fsiSession = FsiEvaluationSession.Create(fsiConfig, args, new StringReader(""), _evalWriter, _evalWriter)

        //TODO review, reuse
        use writer = new StringWriter()
        try
            for file in config.LoadFiles do
                let result, warnings = fsiSession.EvalInteractionNonThrowing (sprintf "#load @\"%s\"" file)
                for w in warnings do writer.WriteLine(strErrorText w)
                match result with | Choice2Of2 exn -> raise exn | _ -> ()

            for file in config.UseFiles do
                let code = File.ReadAllText file
                let result, warnings = fsiSession.EvalInteractionNonThrowing code
                for w in warnings do writer.WriteLine(strErrorText w)
                match result with | Choice2Of2 exn -> raise exn | _ -> ()
        with exn ->
            writer.WriteLine(sprintf "%A" exn)

        fsiSession, writer.ToString(), config

    let eval writer eval x =
        _evalWriter.Writer <- writer
        let result, warnings = eval x
        //! do not leave the temp writer attached, fsi still writes, e.g. when PSF loads assemblies
        _evalWriter.Writer <- _voidWriter
        {
            Warnings = warnings
            Exception =
                match result with
                | Choice2Of2 exn -> exn
                | _ -> null
        }

    static member TryFind(from) =
        sessions |> List.tryFind(fun x -> x.IsFrom(from))

    static member Get(from) =
        match Session.TryFind(from) with
        | Some s -> s
        | _ ->
            sessions <- Session(from) :: sessions
            sessions.Head

    static member Sessions with get() = sessions

    member m.Close() =
        m.OnClose()

        sessions <- sessions |> List.except [m]

        _evalWriter.Dispose()
        _voidWriter.Dispose()
        (fsiSession :> IDisposable).Dispose()

    member val ConfigFile = from

    member x.Config with get() = config

    member x.EditorFile with get() = Path.ChangeExtension(from, ".fsx")

    member x.DisplayName with get() = sprintf "%s - %s" (Path.GetFileName(from)) (Path.GetDirectoryName(from))

    member val Issues = issues

    member val internal OnClose = fun()->() with get, set

    member x.IsFrom(path) =
        String.Equals(from, Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)

    member x.EvalInteraction(writer, code) =
        eval writer fsiSession.EvalInteractionNonThrowing code

    member x.EvalScript(writer, filePath) =
        eval writer fsiSession.EvalScriptNonThrowing filePath

    member x.GetCompletions(longIdent) =
        //! SplitPipeline.SplitPipelineCommand. -> exn
        try
            fsiSession.GetCompletions longIdent
        with _ ->
            Seq.empty

let private mainSessionFrom() =
    Path.Combine(fsfRoaminData(), "main.fs.ini")

/// Gets or creates the main session.
let getMainSession() = Session.Get(mainSessionFrom())

/// Gets the main session or none.
let tryFindMainSession() = Session.TryFind(mainSessionFrom())
