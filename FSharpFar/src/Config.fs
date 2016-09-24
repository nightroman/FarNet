
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Config

open System
open System.IO

type private ConfigSection = NoSection | FscSection | FsiSection

type private KeyValue = {Key : string; Value : string}

type private ConfigData =
    | Empty
    | Comment
    | Section of string
    | Switch of string
    | KeyValue of KeyValue

let private parse (line:string) =
    let text = line.Trim()
    if text.Length = 0 then
        Empty
    elif text.[0] = ';' then
        Comment
    elif text.[0] = '[' then
        if not (text.EndsWith "]") then
            invalidOp "Invalid section, expected '[...]'."
        Section(text.Substring(1, text.Length - 2).Trim())
    else
        let i = text.IndexOf('=')
        if i < 0 then
            Switch text
        else
            KeyValue {
                Key = text.Substring(0, i).Trim()
                Value = text.Substring(i + 1).Trim()
            }

type Config = {
    FscArgs : string[]
    FsiArgs : string[]
    LoadFiles : string[]
    UseFiles : string[]
}

let empty = {FscArgs = [||]; FsiArgs = [||]; LoadFiles = [||]; UseFiles = [||]}

let getConfigFromIniFile path =
    let lines = File.ReadAllLines path
    let root = Path.GetDirectoryName path

    let fscArgs = ResizeArray()
    let fsiArgs = ResizeArray()
    let loadScripts = ResizeArray()
    let useScripts = ResizeArray()

    let mutable currentSection = NoSection
    let mutable lineNo = 0

    let resolve kv =
        let value = Environment.ExpandEnvironmentVariables(kv.Value).Replace("__SOURCE_DIRECTORY__", root)
        match kv.Key with
        | "reference" | "load" | "lib" | "use" when value.StartsWith(".") -> Path.GetFullPath(Path.Combine(root, value))
        | _ -> value

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
                    | _ -> NoSection
            | Switch it ->
                match currentSection with
                | FscSection -> fscArgs.Add("--" + it)
                | FsiSection -> fsiArgs.Add("--" + it)
                | NoSection -> ()
            | KeyValue it ->
                match currentSection with
                | FscSection ->
                    let text = resolve it
                    fscArgs.Add("--" + it.Key + ":" + text)
                | FsiSection ->
                    let text = resolve it
                    match it.Key with
                    | "load" ->
                        loadScripts.Add text
                    | "use" ->
                        useScripts.Add text
                    | _ ->
                        fsiArgs.Add("--" + it.Key + ":" + text)
                | NoSection ->
                    invalidOp "Expected section, found data."
     with e ->
        invalidOp (sprintf "%s(%d): %s" path lineNo e.Message)

    {
        FscArgs = fscArgs.ToArray()
        FsiArgs = fsiArgs.ToArray()
        LoadFiles = loadScripts.ToArray()
        UseFiles = useScripts.ToArray()
    }
