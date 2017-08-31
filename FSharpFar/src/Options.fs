
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Options

open System
open System.IO
open System.Collections.Generic
open Config
open Microsoft.FSharp.Compiler.SourceCodeServices

type FarProjOptions =
    | ConfigOptions of Config
    | ProjectOptions of FSharpProjectOptions

/// Gets cached options from a file.
let private getOptionsFromFile =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<string, DateTime * FarProjOptions> StringComparer.OrdinalIgnoreCase
    fun import path ->
        let time1 = File.GetLastWriteTime path
        let add path = time1, import path
        let update path ((time2, _) as value) = if time1 = time2 then value else add path
        let _, options = cache.AddOrUpdate (path, add, update)
        options

let private getOptionsFromIni = getOptionsFromFile (getConfigFromIniFile >> ConfigOptions)

/// Gets options from .fs.ini
let getOptionsFrom (path: string) =
    getOptionsFromIni path

/// Gets the config path for file.
let getConfigPathForFile path =
    let dir = Path.GetDirectoryName path

    match Directory.GetFiles (dir, "*.fs.ini") with
    | [|file|] ->
        // available custom config
        file
    | _ ->
        // main config
        farMainSessionConfigPath

/// Gets options for a file to be processed.
let getOptionsForFile path =
    getOptionsFrom (getConfigPathForFile path)
