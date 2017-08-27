
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Config

open System
open System.IO

type private ConfigSection = NoSection | FscSection | FsiSection

type private ConfigData =
    | Empty
    | Comment
    | Section of string
    | Switch of string
    | KeyValue of Key : string * Value : string

let private parse (line: string) =
    let text = line.Trim ()
    if text.Length = 0 then
        Empty
    elif text.[0] = ';' then
        Comment
    elif text.[0] = '[' then
        if not (text.EndsWith "]") then
            invalidOp "Invalid section, expected '[...]'."
        Section (text.Substring(1, text.Length - 2).Trim ())
    else
        let i = text.IndexOf '='
        if i < 0 then
            Switch text
        else
            KeyValue (text.Substring(0, i).Trim (), text.Substring(i + 1).Trim ())

type Config = {
    FscArgs: string []
    FsiArgs: string []
    LoadFiles: string []
    UseFiles: string []
}

let empty = {FscArgs = [||]; FsiArgs = [||]; LoadFiles = [||]; UseFiles = [||]}

let private resolve root key value =
    let value = Environment.ExpandEnvironmentVariables(value).Replace ("__SOURCE_DIRECTORY__", root)
    match key with
    | "reference" | "load" | "lib" | "use" when value.StartsWith "." -> Path.GetFullPath (Path.Combine(root, value))
    | _ -> value

let getConfigFromIniFile path =
    let lines = File.ReadAllLines path
    let root = Path.GetDirectoryName path

    let fscArgs = ResizeArray ()
    let fsiArgs = ResizeArray ()
    let loadScripts = ResizeArray ()
    let useScripts = ResizeArray ()

    let mutable currentSection = NoSection
    let mutable lineNo = 0

    let raiseSection () = invalidOp "Expected section [fsc] or [fsi], found data or unknown section."
    try
        for line in lines do
            lineNo <- lineNo + 1
            match parse line with
            | Empty | Comment ->
                ()
            | Section section ->
                currentSection <-
                    match section with
                    | "fsc" -> FscSection
                    | "fsi" -> FsiSection
                    | _ -> raiseSection ()
            | Switch it ->
                match currentSection with
                | FscSection ->
                    fscArgs.Add ("--" + it)
                | FsiSection ->
                    fsiArgs.Add ("--" + it)
                | NoSection ->
                    raiseSection ()
            | KeyValue (key, value) ->
                match currentSection with
                | FscSection ->
                    let text = resolve root key value
                    // use -r instead of --reference to avoid duplicates added by FCS
                    // https://github.com/fsharp/FSharp.Compiler.Service/issues/697
                    match key with
                    | "reference" -> fscArgs.Add ("-r:" + text)
                    | _ -> fscArgs.Add ("--" + key + ":" + text)
                | FsiSection ->
                    let text = resolve root key value
                    match key with
                    | "load" ->
                        loadScripts.Add text
                    | "use" ->
                        useScripts.Add text
                    | _ ->
                        fsiArgs.Add ("--" + key + ":" + text)
                | NoSection ->
                    raiseSection ()
     with e ->
        invalidOp (sprintf "%s(%d): %s" path lineNo e.Message)

    {
        FscArgs = fscArgs.ToArray ()
        FsiArgs = fsiArgs.ToArray ()
        LoadFiles = loadScripts.ToArray ()
        UseFiles = useScripts.ToArray ()
    }
