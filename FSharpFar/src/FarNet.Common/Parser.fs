module FSharpFar.Parser
open System
open FSharp.Compiler.EditorServices

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

let tryCompletions lineStr caret getCompletions =
    let ident = QuickParse.GetPartialLongNameEx(lineStr, caret - 1)
    let name = longIdent ident.QualifyingIdents ident.PartialIdent
    if name.Length = 0 then None else

    let name, replacementIndex =
        if lineStr[caret - 1] = '.' then
            name + ".", caret
        else
            match ident.LastDotPos with
            | Some pos ->
                name, pos + 1
            | None ->
                name, caret - name.Length

    // distinct: Sys[Tab] -> several "System"
    // sort: System.[Tab] -> unsorted
    try
        let completions =
            getCompletions name
            |> Seq.distinct
            |> Seq.sort
            |> Seq.toArray

        Some (name, replacementIndex, ident, completions)
    with _ ->
        None
