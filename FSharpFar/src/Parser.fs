//[rk] c2903ec ("Update deps to be net45 only (#199)", 2017-08-31)
module FSharpFar.Parser

let inline private tryGetLexerSymbolIslands sym =
    if sym.Text.Length = 0 then None else Some (sym.RightColumn, sym.Text.Split '.' |> Array.toList)

// Parsing - find the identifier around the current location
// (we look for full identifier in the backward direction, but only
// for a short identifier forward - this means that when you hover
// 'B' in 'A.B.C', you will get intellisense for 'A.B' module)
let private findIdents col lineStr lookupType =
    Lexer.getSymbol 0 col lineStr lookupType
    |> Option.bind tryGetLexerSymbolIslands

let findLongIdents col lineStr =
    //[rk] case: n1.n2 ; Fuzzy gets [n2], ByLongIdent gets [n1; n2].
    findIdents col lineStr SymbolLookupKind.ByLongIdent

let findLongIdent col (lineStr: string) =
    match Lexer.getSymbol 0 col (lineStr.Substring(0, col)) SymbolLookupKind.ByLongIdent with
    | Some sym -> sym.Text
    | None -> ""

let findLongIdentsAndResidue col (lineStr: string) =
    let lineStr = lineStr.Substring(0, col)
    match Lexer.getSymbol 0 col lineStr SymbolLookupKind.ByLongIdent with
    | Some sym ->
        match sym.Text with
        | "" -> [], ""
        | text ->
            let res = text.Split '.'
            if lineStr.[col - 1] = '.' then
                (Array.toList res), ""
            else
                (Array.toList (Array.sub res 0 (res.Length - 1))), (res.[res.Length - 1])
    | None -> [], ""
