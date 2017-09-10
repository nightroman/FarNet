
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

module FSharpFar.Checker

open System
open System.IO
open Config
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices

type CheckFileResult = {
    Checker: FSharpChecker
    Options: FSharpProjectOptions
    ParseResults: FSharpParseFileResults
    CheckResults: FSharpCheckFileResults
}

let check file text config = async {
    let checker = FSharpChecker.Create ()

    let! options = async {
        // get script options combined with ini, needed for #I, #r and #load in the script
        let ourFlags = [|
            yield! defaultCompilerArgs
            yield! config.FscArgs
            yield! config.EtcArgs
        |]

        // files from config
        let ourFiles = ResizeArray ()
        let addFiles paths =
            for f in paths do
                let f1 = Path.GetFullPath f
                if ourFiles.FindIndex (fun x -> f1.Equals (x, StringComparison.OrdinalIgnoreCase)) < 0 then
                    ourFiles.Add f1
        addFiles config.FscFiles
        addFiles config.EtcFiles

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
    }

    let! parseResults, checkAnswer = checker.ParseAndCheckFileInProject (file, 0, text, options)
    let checkResults =
        match checkAnswer with
        | FSharpCheckFileAnswer.Succeeded x -> x
        | _ -> invalidOp "Unexpected checker abort."

    return {
        Checker = checker
        Options = options
        ParseResults = parseResults
        CheckResults = checkResults
    }
}

let compile (config: Config) = async {
    // assert output is set
    let hasOutOption = config.OutArgs |> Array.exists (fun x -> x.StartsWith "-o:" || x.StartsWith "--out")
    if not hasOutOption then invalidOp "Configuration must have [out] {-o|--out}:<output exe or dll>."

    // combine options    
    let args = [|
        yield "fsc.exe"
        yield! defaultCompilerArgs
        yield! config.FscArgs
        yield! config.OutArgs
        yield! config.FscFiles
        yield! config.OutFiles
    |]

    // compile and get errors and exit code
    let checker = FSharpChecker.Create()
    return! checker.Compile args
}
