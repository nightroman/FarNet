
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Session

open ProxyWriter
open System
open System.IO
open System.Text
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices

let formatFSharpErrorInfo(w : FSharpErrorInfo) =
    let kind = if w.Severity = FSharpErrorSeverity.Warning then "warning" else "error"
    sprintf "%s(%d,%d): %s FS%04d: %s" w.FileName w.StartLineAlternate (w.StartColumn + 1) kind w.ErrorNumber w.Message

type EvalResult =
    {
        Warnings : FSharpErrorInfo[]
        Exception : exn
    }

type Session() =
    // contains some extra "noise" output
    let _voidWriter = new StringWriter()
    // assigned to the session
    let _evalWriter = new ProxyWriter(_voidWriter)
    let args = [|
        getFsiPath()
        "--nologo"
        "--noninteractive"
        sprintf "--lib:%s\\FarNet" (Environment.GetEnvironmentVariable("FARHOME"))
        |]
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let fsiSession = FsiEvaluationSession.Create(fsiConfig, args, new StringReader(""), _evalWriter, _evalWriter)
    
    // Need to invoke something, to trigger and bypass the initial noise output (prompt).
    // In our case, eval FarNet helpers.
    do
        fsiSession.EvalInteraction """
#r "FarNet.dll"
open FarNet
let far=Far.Api
"""

    interface IDisposable with
        member x.Dispose() =
            _evalWriter.Dispose()
            _voidWriter.Dispose()
            (fsiSession :> IDisposable).Dispose()

    member x.Invoke writer command =
        _evalWriter.Writer <- writer
        let result, warnings = fsiSession.EvalInteractionNonThrowing command
        let r = {
            Warnings = warnings
            Exception =
                match result with
                | Choice2Of2 exn -> exn
                | _ -> null
        }
        //! do not leave the temp writer attached, fsi still writes, e.g. when PSF loads assemblies
        _evalWriter.Writer <- _voidWriter
        r

let private _mainSession = lazy (new Session())

/// Global session for fs: commands and interactive 1.
let getMainSession() = _mainSession.Value
