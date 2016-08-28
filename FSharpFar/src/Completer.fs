
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Completer

open System

type Completer(complete) =
    let complete : (string -> seq<string>) = complete

    /// Gets available code completions as:
    /// ok, replacement index, completions.
    member x.GetCompletions(input:string, caret:int) =
        let rec look paren start =
            if start <= 0 then 0 else
            let i = start - 1
            match input.[i] with
            | c when Char.IsLetterOrDigit(c) || c = '.' || c = '_' ->
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
        if start = caret then false, -1, null else

        let name = input.Substring(start, caret - start)
        let iDot = name.LastIndexOf('.')

        // distinct: Sys[Tab] -> several "System"
        // sort: System.[Tab] -> unsorted
        true,
        (if iDot < 0 then start else start + iDot + 1),
        (complete name |> Seq.distinct |> Seq.sort |> Seq.toArray)
