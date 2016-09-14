
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.Kit

open System
open System.IO
open System.Text
open System.Text.RegularExpressions

/// Table keys, e.g. editor.Data
module DataKey =
    let errors = "F# errors"
    let session = "F# session"

let private reNonSpaceWhiteSpace = Regex(@"[\r\n\t]+")

/// Makes a string for one line show.
let strLine(x) = reNonSpaceWhiteSpace.Replace(x, " ")
