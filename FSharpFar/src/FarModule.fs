
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarModule

open FarNet
open Options
open Session
open Microsoft.FSharp.Compiler.SourceCodeServices
open System
open System.IO

module private Key =
    let session = "F# session"
    let errors = "F# errors"
    let autoTips = "F# auto tips"
    let autoCheck = "F# auto check"
    let fsChecking = "fsChecking"

type IEditor with
    member private x.getOpt<'T> (key) =
        let r = x.Data.[key]
        if r = null then None else Some (r :?> 'T)

    member private x.setOpt (key, value) =
        match value with
        | Some v -> x.Data.[key] <- v
        | _ -> x.Data.Remove key

    member x.fsSession
        with get () = x.getOpt<Session> Key.session
        and set (value: Session option) = x.setOpt (Key.session, value)

    member x.fsErrors
        with get () = x.getOpt<FSharpErrorInfo []> Key.errors
        and set (value: FSharpErrorInfo [] option) =
            x.fsChecking <- false
            x.setOpt (Key.errors, value)

    member x.fsAutoTips
        with get () = defaultArg (x.getOpt<bool> Key.autoTips) true
        and set (value: bool) = x.setOpt (Key.autoTips, Some value)

    member x.fsAutoCheck
        with get () = defaultArg (x.getOpt<bool> Key.autoCheck) true
        and set (value: bool) = x.setOpt (Key.autoCheck, Some value)

    member x.fsChecking
        with get () = defaultArg (x.getOpt<bool> Key.fsChecking) false
        and set (value: bool) = x.setOpt (Key.fsChecking, Some value)

    member x.getOptions () =
        match x.fsSession with
        | Some x -> x.Options
        | _ -> getOptionsForFile x.FileName

    member x.tryMyErrors () =
        match x.fsErrors with
        | None -> None
        | Some errors ->

        let file = Path.GetFullPath x.FileName
        let errors = errors |> Array.filter (fun error -> file.Equals (Path.GetFullPath error.FileName, StringComparison.OrdinalIgnoreCase))
        if errors.Length = 0 then None else Some errors
