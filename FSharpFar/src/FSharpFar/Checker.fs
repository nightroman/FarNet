[<RequireQualifiedAccess>]
module FSharpFar.Checker
open System.IO
open FSharp.Compiler.SourceCodeServices

[<NoComparison>]
type CheckFileResult = {
    Checker: FSharpChecker
    Options: FSharpProjectOptions
    ParseResults: FSharpParseFileResults
    CheckResults: FSharpCheckFileResults
}

let check file text config = async {
    let checker = FSharpChecker.Create ()

    let! options = async {
        // config flags
        let flags = [|
            yield! defaultCompilerArgs
            yield! config.FscArgs
            yield! config.EtcArgs
        |]

        // config files and later others
        let files = ResizeArray ()
        let addFiles paths =
            for f in paths do
                let f1 = Path.GetFullPath f
                if not (Seq.containsIgnoreCase f1 files) then
                    files.Add f1
        addFiles config.FscFiles
        addFiles config.EtcFiles

        // .fsx and .fs are different
        if isScriptFileName file then
            // Our flags are used for .fsx #r and #load resolution.
            // SourceFiles: script #load files and the script itself.
            let! options, _errors = checker.GetProjectOptionsFromScript (file, text, otherFlags = Seq.toArray flags)
            
            // add some new files to ours
            addFiles options.SourceFiles
            
            // result options with combined files
            return { options with SourceFiles = files.ToArray () }
        else
            // add .fs file, it may not be in config
            addFiles [file]

            // make input flags
            let args = [|
                yield! flags
                yield! files
            |]
            
            // options from just our flags
            return checker.GetProjectOptionsFromCommandLineArgs (file, args)
    }

    let! parseResults, checkAnswer = checker.ParseAndCheckFileInProject (file, 0, text, options)
    let checkResults =
        match checkAnswer with
        | FSharpCheckFileAnswer.Succeeded x -> x
        | FSharpCheckFileAnswer.Aborted -> invalidOp "Unexpected checker abort."

    return {
        Checker = checker
        Options = options
        ParseResults = parseResults
        CheckResults = checkResults
    }
}

let compile config = async {
    // assert output is set
    let hasOutOption = config.OutArgs |> List.exists (fun x -> x.StartsWith "-o:" || x.StartsWith "--out:")
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
