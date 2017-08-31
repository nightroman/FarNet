
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Checker

open System
open System.IO
open Config
open Options
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices

type CheckFileResult = {
    Checker: FSharpChecker
    Options: FSharpProjectOptions
    ParseResults: FSharpParseFileResults
    CheckResults: FSharpCheckFileResults
}

let check file text options = async {
    let checker = FSharpChecker.Create ()

    let! projOptions = async {
        match options with
            | ConfigOptions config ->
                // get script options combined with ini, needed for #I, #r and #load in the script
                let ourFlags = [|
                    // our default args
                    yield! defaultCompilerArgs
                    // user fsc args
                    yield! config.FscArgs
                |]

                // #load files from config
                let ourFiles = ResizeArray config.LoadFiles
                let addFiles paths =
                    for f in paths do
                        let f1 = Path.GetFullPath f
                        if ourFiles.FindIndex (fun x -> f1.Equals (x, StringComparison.OrdinalIgnoreCase)) < 0 then
                            ourFiles.Add f1

                if isScriptFileName file then
                    // GetProjectOptionsFromScript gets script #load files and the script itself as SourceFiles
                    let! options, _errors = checker.GetProjectOptionsFromScript (file, text, otherFlags = ourFlags)
                    return
                        { options with
                            SourceFiles =
                                [|
                                    yield! ourFiles
                                    yield! options.SourceFiles
                                |]
                        }
                else
                    // add the file itself, case: it is not in .LoadFiles
                    addFiles [file]
                    let args = [|
                        yield! ourFlags
                        yield! ourFiles
                    |]
                    return checker.GetProjectOptionsFromCommandLineArgs (file, args)
            | ProjectOptions options ->
                return options
    }

    let! parseResults, checkAnswer = checker.ParseAndCheckFileInProject (file, 0, text, projOptions)
    let checkResults =
        match checkAnswer with
        | FSharpCheckFileAnswer.Succeeded x -> x
        | _ -> invalidOp "Unexpected checker abort."

    return {
        Checker = checker
        Options = projOptions
        ParseResults = parseResults
        CheckResults = checkResults
    }
}
