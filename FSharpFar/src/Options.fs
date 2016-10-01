
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
            cache.Add (path, (newStamp, r))
            r

let private getOptionsFromIni = getOptionsFromFile (getConfigFromIniFile >> ConfigOptions)
let private getOptionsFromProj = getOptionsFromFile (ProjectCracker.GetProjectOptionsFromProjectFile >> ProjectOptions)

/// Gets options from .fsproj or INI.
let getOptionsFrom (path: string) =
    if path.EndsWith (".fsproj", StringComparison.OrdinalIgnoreCase) then
        getOptionsFromProj path
    else
        getOptionsFromIni path

/// Gets options for a file to be processed.
let getOptionsForFile path =
    let dir = Path.GetDirectoryName path

    match Directory.GetFiles (dir, "*.fs.ini") with
    | [|file|] ->
        getOptionsFromIni file
    | _ ->

    match Directory.GetFiles (dir, "*.fsproj") with
    | [|file|] ->
        getOptionsFromProj file
    | _ ->

    getOptionsFromIni (mainSessionConfigPath ())
