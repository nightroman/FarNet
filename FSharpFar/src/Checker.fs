
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.Checker

open System
open System.IO
open Config
open Session
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

type FarProjOptions =
    | ProjectOptions of FSharpProjectOptions
    | ConfigOptions of Config

let private cacheProjectOptions = System.Collections.Generic.Dictionary<string, DateTime * FSharpProjectOptions>(StringComparer.OrdinalIgnoreCase)

let getOptionsForFile fileName (session: Session option) =
    assert isFSharpFileName fileName

    if session.IsSome then ConfigOptions session.Value.Config
    else

    let dir = Path.GetDirectoryName(fileName)

    let ini = Directory.GetFiles(dir, "*.fs.ini")
    //TODO cache ini, too
    if ini.Length = 1 then ConfigOptions (getConfigurationFromFile ini.[0])
    else

    let proj = Directory.GetFiles(dir, "*.fsproj")
    if proj.Length = 1 then
        let projPath = proj.[0]
        let newStamp = File.GetLastWriteTime projPath
        let ok, it = cacheProjectOptions.TryGetValue projPath
        if ok && newStamp = fst it then
            ProjectOptions (snd it)
        else
            let projOptions = ProjectCracker.GetProjectOptionsFromProjectFile projPath
            cacheProjectOptions.Add(projPath, (newStamp, projOptions))
            ProjectOptions projOptions
    else

    ConfigOptions (getMainSession().Config) //TODO we do not have a new session, config is enough...

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
