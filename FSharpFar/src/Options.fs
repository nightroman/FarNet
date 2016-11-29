
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

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
    let cache = Dictionary<string, DateTime * FarProjOptions> StringComparer.OrdinalIgnoreCase
    fun getOptions path ->
        let newStamp = File.GetLastWriteTime path
        let ok, it = cache.TryGetValue path
        if ok && newStamp = fst it then
            snd it
        else
            let r = getOptions path
            cache.[path] <- (newStamp, r)
            r

let private getOptionsFromIni = getOptionsFromFile (getConfigFromIniFile >> ConfigOptions)

let private getOptionsFromProj path =
    try
        getOptionsFromFile (ProjectCracker.GetProjectOptionsFromProjectFile >> ProjectOptions) path
    with exn ->
        raise (InvalidOperationException ("Cannot process .fsproj. Make sure MSBuild is installed and .fsproj is valid. Or use .fs.ini.", exn))

/// Gets options from .fsproj or INI.
let getOptionsFrom (path: string) =
    if path.EndsWith (".fsproj", StringComparison.OrdinalIgnoreCase) then
        getOptionsFromProj path
    else
        getOptionsFromIni path

/// Gets the config path for file.
let getConfigPathForFile path =
    let dir = Path.GetDirectoryName path

    // use available custom config
    match Directory.GetFiles (dir, "*.fs.ini") with
    | [|file|] ->
        file
    | _ ->

    // main config for scripts
    if isScriptFileName path then farMainSessionConfigPath else

    // available F# project
    match Directory.GetFiles (dir, "*.fsproj") with
    | [|file|] ->
        file
    | _ ->

    // main config
    farMainSessionConfigPath

/// Gets options for a file to be processed.
let getOptionsForFile path =
    getOptionsFrom (getConfigPathForFile path)
