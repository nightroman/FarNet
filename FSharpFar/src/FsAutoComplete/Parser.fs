//[rk] c2903ec ("Update deps to be net45 only (#199)", 2017-08-31)

namespace FsAutoComplete
open System

/// Parsing utilities for IntelliSense (e.g. parse identifier on the left-hand side
/// of the current cursor location etc.)
module Parsing =
  let inline private tryGetLexerSymbolIslands sym =
      match sym.Text with
      | "" -> None
      | _ -> Some (sym.RightColumn, sym.Text.Split '.' |> Array.toList)

  // Parsing - find the identifier around the current location
  // (we look for full identifier in the backward direction, but only
  // for a short identifier forward - this means that when you hover
  // 'B' in 'A.B.C', you will get intellisense for 'A.B' module)
  let findIdents col lineStr lookupType =
      if lineStr = "" then None
      else
          Lexer.getSymbol 0 col lineStr lookupType [||]
          |> Option.bind tryGetLexerSymbolIslands

  let findLongIdents (col, lineStr) =
    //[rk] case: n1.n2 ; Fuzzy gets [n2], ByLongIdent gets [n1; n2].
    findIdents col lineStr SymbolLookupKind.ByLongIdent

  let findLongIdentsAndResidue (col, lineStr:string) =
      let lineStr = lineStr.Substring(0, col)

      match Lexer.getSymbol 0 col lineStr SymbolLookupKind.ByLongIdent [||] with
      | Some sym ->
          match sym.Text with
          | "" -> [], ""
          | text ->
              let res = text.Split '.' |> List.ofArray |> List.rev
              if lineStr.[col - 1] = '.' then res |> List.rev, ""
              else
                  match res with
                  | head :: tail -> tail |> List.rev, head
                  | [] -> [], ""
      | _ -> [], ""
