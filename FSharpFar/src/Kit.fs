
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.Kit

open System
open System.IO
open System.Text

let private _FsiPath =
    lazy (
        let folders = Environment.GetEnvironmentVariable("PATH").Split(';')
        match Array.tryPick (fun x ->
            let r = Path.Combine(x, "fsi.exe")
            if File.Exists(r) then Some r
            else None) folders with
        | Some path -> path
        | None -> invalidOp "fsi.exe is not found in the path."
    )

/// Gets the full path of fsi.exe or throws if it is not found.
let getFsiPath() = _FsiPath.Value
