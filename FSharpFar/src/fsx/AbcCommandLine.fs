module internal FSharpFar.CommandLine

open System
open System.IO

/// Gets exe, ?ini, ?fsx, args as they are.
let parseCommandLineArgs (args: string[]) =
    // [0] - app.exe
    let exe = args.[0]

    // [1] - ?.ini, skip it
    let ini, index =
        if args.Length > 1 && args.[1].ToLower().EndsWith(".ini") then
            Some args.[1], 2
        else
            None, 1

    let r = ResizeArray()
    let mutable fsx = None
    let mutable keepParsing = true

    for i in index .. args.Length - 1 do
        let x = args.[i]
        r.Add(x)
        if fsx.IsNone && keepParsing then
            if not (x.StartsWith("-") || x.StartsWith("/")) then
                fsx <- Some x
            else if x = "--" then
                keepParsing <- false

    exe, ini, fsx, r.ToArray()

/// Try get full path of ini given ?ini and ?fsx.
let tryResolveIni ini fsx =
    match ini with
    | Some ini ->
        Some (Path.GetFullPath(ini))
    | None ->
        Config.tryFindFileInDirectory (
            match fsx with
            | None ->
                Environment.CurrentDirectory
            | Some fsx ->
                Path.GetDirectoryName(Path.GetFullPath(fsx))
        )
