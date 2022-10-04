[<RequireQualifiedAccess>]
module FSharpFar.Command
open System
open System.Data.Common
open System.Text.RegularExpressions

type ProjectOpenBy = VS|VSCode with
    static member Parse(s) =
        let s = defaultArg s "VS"
        match s with
        | "VS" -> VS
        | "VSCode" -> VSCode
        | _ -> failwith $"Invalid value '{s}'. Valid values: VS|VSCode."

type ProjectOutput = Normal|Script with
    static member Parse(s) =
        let s = defaultArg s "Normal"
        match s with
        | "Normal" -> Normal
        | "Script" -> Script
        | _ -> failwith $"Invalid value '{s}'. Valid values: Normal|Script."

type Data =
    | Quit
    | Code of string
    | Open of OpenArgs
    | Exec of ExecArgs
    | Compile of OpenArgs
    | Project of ProjectArgs

and OpenArgs =
    { With : string option }

and ProjectArgs =
    { With : string option; Open : ProjectOpenBy; Type : ProjectOutput }

and ExecArgs =
    { With : string option; File : string option; Code : string option }

let private tryPopString key (sb: DbConnectionStringBuilder) =
    match sb.TryGetValue key with 
    | true, value ->
        sb.Remove key |> ignore
        Some(value :?> string)
    | _ ->
        None

let private command name rest sb =
    match name with
    | "open" ->
        Open {
            With = tryPopString "with" sb |> Option.map farResolvePath
        }
    | "exec" ->
        Exec {
            With = tryPopString "with" sb |> Option.map farResolvePath
            File = tryPopString "file" sb |> Option.map farResolvePath
            Code = if String.IsNullOrWhiteSpace rest then None else Some rest
        }
    | "compile" ->
        Compile {
            With = tryPopString "with" sb |> Option.map farResolvePath
        }
    | "project" ->
        Project {
            With = tryPopString "with" sb |> Option.map farResolvePath
            Open = tryPopString "open" sb |> ProjectOpenBy.Parse
            Type = tryPopString "type" sb |> ProjectOutput.Parse
        }
    | _ ->
        failwithf "Unknown command '%s'." name

let private reCommand = Regex @"^\s*(?://(?<name>\w+)|(?<name>\w+):)\s*(?<rest>.*)"
let private reQuit = Regex @"^\s*#quit\b"

/// Parses the module command "fs:".
/// Failure ~ user error.
let parse text =
    let matchCommand = reCommand.Match text
    if matchCommand.Success then
        let commandName = matchCommand.Groups["name"].Value
        let rest = matchCommand.Groups["rest"].Value

        let index = rest.IndexOf ";;"
        let part1, part2 = if index < 0 then rest, "" else rest.Substring(0, index), rest.Substring(index + 2)

        let sb = FarNet.Works.Kit.ParseParameters(part1)

        let r = command commandName part2 sb

        if sb.Count > 0 then
            failwithf "Unknown '%s' keys: %O" (r.GetType().Name.ToLower()) sb
        r
    elif reQuit.IsMatch text then
        Quit
    else
        Code text
