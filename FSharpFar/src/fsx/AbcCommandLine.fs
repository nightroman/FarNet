module internal FSharpFar.CommandLine

open System
open System.IO

/// Gets exe, ?ini, ?fsx, FCS args.
/// fsx is either invoked .fsx or the last of .fs|--use|--load files.
let parseCommandLineArgs (args: string[]) =
    // [0] - app.exe
    let exe = args[0]

    // [1] ~ .ini? done
    if args.Length > 1 && args[1].EndsWith(".ini", StringComparison.OrdinalIgnoreCase) then
        exe, Some args[1], None, Array.skip 2 args
    else

    let mutable i = 1
    let mutable fsx = None
    let mutable lastSource = None
    let mutable keepParsing = true

    while keepParsing && i < args.Length do
        let x = args[i]
        i <- i + 1

        //| source file
        if not (x.StartsWith("-") || x.StartsWith("/")) then
            if x.EndsWith(".fsx", StringComparison.OrdinalIgnoreCase) then
                // fsx ... my.fsx <arguments>
                fsx <- Some x
                keepParsing <- false
            else
                // fsx ... my.fs ...
                lastSource <- Some x
        //| --use
        else if x.StartsWith("--use:") then
            lastSource <- Some(x.Substring(6))
        //| --load
        else if x.StartsWith("--load:") then
            lastSource <- Some(x.Substring(7))
        //| --
        else if x = "--" then
            keepParsing <- false

    exe, None, (if fsx.IsSome then fsx else lastSource), Array.skip 1 args

/// Try get full path of ini given ?ini and ?fsx.
let tryResolveIni ini fsx =
    match ini with
    | Some ini ->
        Some(Path.GetFullPath(ini))
    | None ->
        Config.tryFindFileInDirectory (
            match fsx with
            | None ->
                Environment.CurrentDirectory
            | Some fsx ->
                Path.GetDirectoryName(Path.GetFullPath(fsx))
        )
