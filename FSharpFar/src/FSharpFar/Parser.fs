module FSharpFar.Parser
open System
open Microsoft.FSharp.Compiler

/// Joins full names and partial with ".".
let longIdent (idents: string list) (partial: string) =
    if idents.IsEmpty then
        partial
    else
        let idents = String.Join (".", idents)
        if partial.Length = 0 then
            idents
        else
            idents + "." + partial

let findLongIdents caret lineStr =
    match QuickParse.GetCompleteIdentifierIsland true lineStr caret with
    | None ->
        None
    | Some (str, pos, x) ->
        let names, partial = QuickParse.GetPartialLongName (str, (str.Length - 1))
        Some (pos, names @ [ partial ])
