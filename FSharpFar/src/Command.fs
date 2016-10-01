
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Command

open System
open System.Data.Common
open System.Text.RegularExpressions

let reCommand = Regex @"^\s*//(\w+)\s*(.*)"
let reQuit = Regex @"^\s*#quit\b"

type OpenArgs = {With : string option}
type ExecArgs = {File : string; With : string option; Code : string option}

type Command =
    | Quit
    | Code of string
    | Open of OpenArgs
    | Exec of ExecArgs

let popString key (sb: DbConnectionStringBuilder) =
    let is, it = sb.TryGetValue key
    if is then
        sb.Remove key |> ignore
        it :?> string
    else
        invalidOp (sprintf "Missing mandatory key '%s'." key)

let popStringOpt key (sb: DbConnectionStringBuilder) =
    let is, it = sb.TryGetValue key
    if is then
        sb.Remove key |> ignore
        Some (it :?> string)
    else
        None

let command name rest sb =
    match name with
    | "open" ->
        Open {
            With = popStringOpt "with" sb
        }
    | "exec" ->
        Exec {
            File = popString "file" sb
            With = popStringOpt "with" sb
            Code = if String.IsNullOrWhiteSpace rest then None else Some rest
        }
    | _ ->
        invalidOp <| sprintf "Unknown command '%s'." name

let parseCommand text =
    let maCommand = reCommand.Match text
    if not maCommand.Success then
        if reQuit.IsMatch text then
            Quit
        else
            Code text
    else

    let commandName = maCommand.Groups.[1].Value
    let rest = maCommand.Groups.[2].Value

    let split = ";;"
    let index = rest.IndexOf split
    let part1, part2 = if index < 0 then rest, "" else rest.Substring (0, index), rest.Substring (index + split.Length)

    let sb = DbConnectionStringBuilder ()
    sb.ConnectionString <- part1

    let r = command commandName part2 sb

    if sb.Count > 0 then invalidOp <| sprintf "Unknown '%s' keys: %s" (r.GetType().Name.ToLower ()) (sb.ToString ())
    r
