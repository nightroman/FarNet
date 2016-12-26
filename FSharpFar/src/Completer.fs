
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Completer

open System

/// Gets available code completions as:
/// Some (replacementIndex, completions)
let complete getCompletions (input: string) (caret: int) =
    let rec look paren start =
        if start <= 0 then 0 else
        let i = start - 1
        match input.[i] with
        | c when Char.IsLetterOrDigit c || c = '.' || c = '_' || c = '`' ->
            look paren i
        | '(' | '{' | '[' ->
            if paren > 0 then
                look (paren - 1) i
            else
                start
        | '}' | ')' | ']' ->
            look (paren + 1) i
        | _ when paren > 0 ->
            look paren i
        | _ -> start
    let start = look 0 caret
    if start = caret then None else

    let name = input.Substring (start, caret - start)
    let iDot = name.LastIndexOf '.'

    //_161108_054202
    let name = name.Replace ("``", "")

    // distinct: Sys[Tab] -> several "System"
    // sort: System.[Tab] -> unsorted
    Some (
        (if iDot < 0 then start else start + iDot + 1),
        (getCompletions name |> Seq.distinct |> Seq.sort |> Seq.toArray)
    )
