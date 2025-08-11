
[<RequireQualifiedAccess>]
module FSharpFar.Command
open System
open FarNet

#nowarn "57"

type ProjectOpenBy = VS|VSCode with
    static member Parse(s) =
        match s with
        | null -> VS
        | "VS" -> VS
        | "VSCode" -> VSCode
        | _ -> failwith $"Invalid value '{s}'. Valid values: VS, VSCode."

type ProjectOutput = Normal|Script with
    static member Parse(s) =
        match s with
        | null -> Normal
        | "Normal" -> Normal
        | "Script" -> Script
        | _ -> failwith $"Invalid value '{s}'. Valid values: Normal, Script."

type Data =
    | Quit
    | Code of string
    | Open of OpenArgs
    | Exec of ExecArgs
    | Compile of CompileArgs
    | Project of ProjectArgs

and OpenArgs =
    { With : string option }

and CompileArgs =
    { With : string option }

and ProjectArgs =
    { With : string option; Open : ProjectOpenBy; Type : ProjectOutput }

and ExecArgs =
    { With : string option; File : string option; Code : string option }

let private tryPath key (parameters: inref<CommandParameters>) =
    match parameters.GetPath(key) with 
    | null ->
        None
    | value ->
        Some value

/// Parses the module command "fs:".
let parse text =
    let parameters = CommandParameters.Parse(text, true)
    let command = parameters.Command
    let text = parameters.Text

    if parameters.Command.Length = 0 then
        if text.StartsWith "#quit" then
            Quit
        else
            Code (text.ToString())
    else
    let command =
        if command.SequenceEqual "exec" then
            Exec {
                With = tryPath "with" &parameters
                File = tryPath "file" &parameters
                Code = if text.Length = 0 then None else Some (text.ToString())
            }
        else
        if command.SequenceEqual "project" then
            Project {
                With = tryPath "with" &parameters
                Open = try parameters.GetString "open" |> ProjectOpenBy.Parse with ex -> raise (parameters.ParameterError("open", ex.Message))
                Type = try parameters.GetString "type" |> ProjectOutput.Parse with ex -> raise (parameters.ParameterError("type", ex.Message))
            }
        else
        if command.SequenceEqual "compile" then
            Compile {
                With = tryPath "with" &parameters
            }
        else
        if command.SequenceEqual "open" then
            Open {
                With = tryPath "with" &parameters
            }
        else
            raise (ModuleException $"Unknown command '{command.ToString()}'.")

    parameters.ThrowUnknownParameters()
    command
