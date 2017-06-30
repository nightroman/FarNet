
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

[<AutoOpen>]
module FSharpFar.Kit

open System
open System.Text.RegularExpressions

/// Makes a string for one line show.
let strAsLine =
    let re = Regex @"[\r\n\t]+"
    fun x -> re.Replace (x, " ")

/// Zips 2+ spaces into one.
let strZipSpace =
    let re = Regex @"[ \t]{2,}"
    fun x -> re.Replace (x, " ")

/// A function that always returns the same value.
let inline always value = fun _ -> value

/// Gets true if a char is an identifier char.
let isIdentChar char = Char.IsLetterOrDigit char || char = '_' || char = '\''

/// Gets true if a char is a long identifier char.
let isLongIdentChar char = isIdentChar char || char = '.'

/// Gets true if a string is a normal identifier.
let isIdentStr str = String.forall isIdentChar str
