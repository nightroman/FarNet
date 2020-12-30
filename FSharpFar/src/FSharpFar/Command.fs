[<RequireQualifiedAccess>]
module FSharpFar.Command
open System
open System.Data.Common
open System.Text.RegularExpressions

type Data =
    | Quit
    | Code of string
    | Open of OpenArgs
    | Exec of ExecArgs
    | Compile of OpenArgs
and OpenArgs =
    { With : string option }
and ExecArgs =
    { With : string option; File : string option; Code : string option }

let private tryPopString key (sb: DbConnectionStringBuilder) =
    match sb.TryGetValue key with 
    | true, value ->
        sb.Remove key |> ignore
        Some (value :?> string)
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
    | _ ->
        failwithf "Unknown command '%s'." name

let private reCommand = Regex @"^\s*//(\w+)\s*(.*)"
let private reQuit = Regex @"^\s*#quit\b"

/// Parses the module command "fs:".
/// Failure ~ user error.
let parse text =
    let matchCommand = reCommand.Match text
    if matchCommand.Success then
        let commandName = matchCommand.Groups.[1].Value
        let rest = matchCommand.Groups.[2].Value

        let split = ";;"
        let index = rest.IndexOf split
        let part1, part2 = if index < 0 then rest, "" else rest.Substring (0, index), rest.Substring (index + split.Length)

        let sb = DbConnectionStringBuilder ()
        sb.ConnectionString <- part1

        let r = command commandName part2 sb

        if sb.Count > 0 then
            failwithf "Unknown '%s' keys: %O" (r.GetType().Name.ToLower ()) sb
        r
    elif reQuit.IsMatch text then
        Quit
    else
        Code text
