
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Options

open System
open System.IO
open Config
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

type FarProjOptions =
    | ProjectOptions of FSharpProjectOptions
    | ConfigOptions of Config

let private cacheFarProjOptions = System.Collections.Generic.Dictionary<string, DateTime * FarProjOptions>(StringComparer.OrdinalIgnoreCase)

/// Gets cached options from a file.
let private getOptionsFromFile getOptions path =
    let newStamp = File.GetLastWriteTime path
    let ok, it = cacheFarProjOptions.TryGetValue path
    if ok && newStamp = fst it then
        snd it
    else
        let r = getOptions path
        cacheFarProjOptions.Add(path, (newStamp, r))
        r

/// Gets options from .fsproj or INI.
let getOptionsFrom path =
    if String.Equals (Path.GetExtension path, ".fsproj", StringComparison.OrdinalIgnoreCase) then
        getOptionsFromFile (ProjectCracker.GetProjectOptionsFromProjectFile >> ProjectOptions) path
    else
        getOptionsFromFile (getConfigFromIniFile >> ConfigOptions) path

/// Gets options for a file to be processed.
let getOptionsForFile path =
    assert isFSharpFileName path
    let dir = Path.GetDirectoryName (path)

    match Directory.GetFiles(dir, "*.fs.ini") with
    | [|file|] ->
        getOptionsFromFile (getConfigFromIniFile >> ConfigOptions) file
    | _ ->

    match Directory.GetFiles(dir, "*.fsproj") with
    | [|file|] ->
        getOptionsFromFile (ProjectCracker.GetProjectOptionsFromProjectFile >> ProjectOptions) file
    | _ ->

    getOptionsFromFile (getConfigFromIniFile >> ConfigOptions) (mainSessionConfigPath())
