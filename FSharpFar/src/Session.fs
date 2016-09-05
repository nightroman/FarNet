
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Session

open ProxyWriter
open System
open System.IO
open System.Text
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Interactive.Shell

type EvalResult =
    {
        Warnings : FSharpErrorInfo[]
        Exception : exn
    }

let formatFSharpErrorInfo(w : FSharpErrorInfo) =
    let kind = if w.Severity = FSharpErrorSeverity.Warning then "warning" else "error"
    sprintf "%s(%d,%d): %s FS%04d: %s" w.FileName w.StartLineAlternate (w.StartColumn + 1) kind w.ErrorNumber w.Message

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
        writer.WriteLine(formatFSharpErrorInfo w)
    if r.Exception <> null then
        writer.WriteLine(sprintf "%A" r.Exception)

type Session private (from) =
    static let mutable sessions : Session list = []
    let from = Path.GetFullPath from

    // contains some extra "noise" output
    let _voidWriter = new StringWriter()
    // assigned to the session
    let _evalWriter = new ProxyWriter(_voidWriter)

    let fsiSession, issues =
        let configArgs, loadScripts, useScripts =
            if File.Exists from then Config.getConfigurationFromFile from else [||], [||], [||]
        let defaultArgs = [|
            getFsiPath()
            "--nologo"
            "--noninteractive"
            sprintf @"--lib:%s\FarNet" (Environment.GetEnvironmentVariable("FARHOME"))
            |]
        let args = Array.append defaultArgs configArgs
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

        // collectible=true has issues, see e-note
        let fsiSession = FsiEvaluationSession.Create(fsiConfig, args, new StringReader(""), _evalWriter, _evalWriter)

        //TODO review, reuse
        use writer = new StringWriter()
        try
            for file in loadScripts do
                let result, warnings = fsiSession.EvalInteractionNonThrowing (sprintf "#load @\"%s\"" file)
                for w in warnings do writer.WriteLine(formatFSharpErrorInfo w)
                match result with | Choice2Of2 exn -> raise exn | _ -> ()

            for file in useScripts do
                let code = File.ReadAllText file
                let result, warnings = fsiSession.EvalInteractionNonThrowing code
                for w in warnings do writer.WriteLine(formatFSharpErrorInfo w)
                match result with | Choice2Of2 exn -> raise exn | _ -> ()
        with exn ->
            writer.WriteLine(sprintf "%A" exn)

        fsiSession, writer.ToString()

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
    Path.Combine(fsfRoaminData(), "main.fsi.ini")

/// Gets or creates the main session.
let getMainSession() = Session.Get(mainSessionFrom())

/// Gets the main session or none.
let tryFindMainSession() = Session.TryFind(mainSessionFrom())
