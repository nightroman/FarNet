
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.Kit

open System
open System.IO
open System.Text
open System.Text.RegularExpressions

let private reNonSpaceWhiteSpace = Regex(@"[\r\n\t]+")

/// Makes a string for one line show.
let strLine(x) = reNonSpaceWhiteSpace.Replace(x, " ")

/// A function that always returns the same value.
let inline always value = fun _ -> value

/// Gets true if a char is an identifier char.
let isIdentChar char = Char.IsLetterOrDigit char || char = '_'

/// Gets true if a char is a long identifier char.
let isLongIdentChar char = Char.IsLetterOrDigit char || char = '_' || char = '.'
