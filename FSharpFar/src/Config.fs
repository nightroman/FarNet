
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Config

open System
open System.IO

type private ConfigSection = NoSection | FsiSection

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

let getConfigurationFromFile path =
    let lines = File.ReadAllLines path
    let root = Path.GetDirectoryName path

    let loadScripts = ResizeArray()
    let useScripts = ResizeArray()
    let args = ResizeArray()

    let mutable currentSection = NoSection
    let mutable lineNo = 0

    try
        for line in lines do
            lineNo <- lineNo + 1
            match parse line with
            | Empty | Comment -> ()
            | Section section ->
                currentSection <- if section = "fsi" then FsiSection else NoSection
            | Switch it ->
                if currentSection = FsiSection then
                    args.Add("--" + it)
            | KeyValue it ->
                if currentSection = FsiSection then
                    let text = Environment.ExpandEnvironmentVariables(it.Value).Replace("__SOURCE_DIRECTORY__", root)
                    match it.Key with
                    | "load" ->
                        loadScripts.Add text
                    | "use" ->
                        useScripts.Add text
                    | _ ->
                        args.Add("--" + it.Key + ":" + text)
     with e ->
        invalidOp (sprintf "%s(%d): %s" path lineNo e.Message)

    args.ToArray(), loadScripts.ToArray(), useScripts.ToArray()
