
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.EditorExt

open FarNet
open System.IO
open Config
open Session
open Microsoft.FSharp.Compiler

module private Key =
    let session = "F# session"
    let errors = "F# errors"

type IEditor with
    member private x.getOpt<'T>(key) =
        let r = x.Data.[key]
        if r = null then None else Some (r :?> 'T)
    
    member private x.setOpt(key, value) =
        match value with
        | Some v -> x.Data.[key] <- v
        | _ -> x.Data.Remove(key)

    member x.fsSession
        with get() = x.getOpt<Session> Key.session
        and set (value : Session option) = x.setOpt(Key.session, value)
    
    member x.fsErrors
        with get() = x.getOpt<FSharpErrorInfo[]> Key.errors
        and set (value : FSharpErrorInfo[] option) = x.setOpt(Key.errors, value)

    member x.fsConfig() =
        assert isFSharpFileName x.FileName
        match x.fsSession with
        | Some ses ->
            ses.Config
        | _ ->
            let dir = Path.GetDirectoryName(x.FileName)
            let ini = Directory.GetFiles(dir, "*.fs.ini")
            match ini with
            | [|configFile|] ->
                getConfigurationFromFile configFile
            | _ ->
                getMainSession().Config //TODO we do not have a new session, config may be enough...
