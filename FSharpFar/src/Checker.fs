
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Checker

open System
open System.IO
open Config
open Options
open Session
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

let check file text options =
    let checker = FSharpChecker.Create()

    let projOptions =
        match options with
            | ProjectOptions options -> options
            | ConfigOptions config ->
                // to use it combined with ini, needed for native refs and #load
                let projOptionsFile = checker.GetProjectOptionsFromScript(file, text) |> Async.RunSynchronously

                let files = ResizeArray()
                let addFiles arr =
                    for f in arr do
                        let f1 = Path.GetFullPath(f)
                        if files.FindIndex(fun x -> f1.Equals(x, StringComparison.OrdinalIgnoreCase)) < 0 then
                            files.Add(f1)

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
                    yield! getCompilerOptions()
                    yield! files
                |]
                checker.GetProjectOptionsFromCommandLineArgs(file, args)

    let parseResults, checkAnswer = checker.ParseAndCheckFileInProject(file, 0, text, projOptions) |> Async.RunSynchronously
    let checkResults =
        match checkAnswer with
        | FSharpCheckFileAnswer.Succeeded x -> x
        | _ -> failwith "unexpected aborted"

    parseResults, checkResults

let strTip tip =
    use w = new StringWriter()

    // see FCS buildFormatComment
    let writeXmlDoc cmt =
        match cmt with
        | FSharpXmlDoc.Text s -> w.WriteLine s
        | FSharpXmlDoc.XmlDocFileSignature (file, signature) -> () //TODO see FSAC
        | FSharpXmlDoc.None -> ()

    match tip with
    | FSharpToolTipText list ->
        for item in list do
            match item with
            // String.Length
            | FSharpToolTipElement.Single (s, x) ->
                w.WriteLine s
                writeXmlDoc x
            // no examples yet, FCS ignores 3rd
            | FSharpToolTipElement.SingleParameter (s, x, _) ->
                w.WriteLine s
                writeXmlDoc x
            // String.Substring
            | FSharpToolTipElement.Group list ->
                for (s, x) in list do
                    w.WriteLine s
                    writeXmlDoc x
            | FSharpToolTipElement.CompositionError err ->
                w.WriteLine err
            | FSharpToolTipElement.None ->
                ()

    w.ToString()
