
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Checker

open System
open System.IO
open Config
open Options
open Session
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices

type CheckFileResult = {
    Checker: FSharpChecker
    Options: FSharpProjectOptions
    ParseResults: FSharpParseFileResults
    CheckResults: FSharpCheckFileResults
}

let check file text options =
    let checker = FSharpChecker.Create ()

    let projOptions =
        match options with
            | ProjectOptions options -> options
            | ConfigOptions config ->
                // to use it combined with ini, needed for native refs and #load
                let projOptionsFile = checker.GetProjectOptionsFromScript (file, text) |> Async.RunSynchronously

                let files = ResizeArray ()
                let addFiles arr =
                    for f in arr do
                        let f1 = Path.GetFullPath f
                        if files.FindIndex (fun x -> f1.Equals (x, StringComparison.OrdinalIgnoreCase)) < 0 then
                            files.Add f1

                addFiles config.LoadFiles
                addFiles config.UseFiles
                // #load files and the file itself
                addFiles projOptionsFile.ProjectFileNames

                let args = [|
                    // "default" options and references
                    yield! projOptionsFile.OtherOptions
                    // user fsc
                    yield! config.FscArgs
                    // our fsc
                    yield! getCompilerOptions ()
                    yield! files
                |]
                checker.GetProjectOptionsFromCommandLineArgs (file, args)

    let parseResults, checkAnswer = checker.ParseAndCheckFileInProject (file, 0, text, projOptions) |> Async.RunSynchronously
    let checkResults =
        match checkAnswer with
        | FSharpCheckFileAnswer.Succeeded x -> x
        | _ -> failwith "unexpected aborted"

    {
        Checker = checker
        Options = projOptions
        ParseResults = parseResults
        CheckResults = checkResults
    }

let strTip tip =
    use w = new StringWriter ()

    let data = TipFormatter.formatTip tip
    for list in data do
        for (signature, comment) in list do
            w.WriteLine signature
            if not (String.IsNullOrEmpty comment) then
                if not (comment.StartsWith Environment.NewLine) then
                    w.WriteLine ()
                w.WriteLine (strZipSpace comment)

    w.ToString ()
