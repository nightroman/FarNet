module FSharpFar.Config
open System
open System.IO
open System.Xml

/// Configuration data for checkers and sessions.
type Config = {
    FscArgs: string list
    FscFiles: string list
    FsiArgs: string list
    FsiFiles: string list
    UseFiles: string list
    OutArgs: string list
    OutFiles: string list
    EtcArgs: string list
    EtcFiles: string list
}

/// Empty configuration.
let empty = {
    FscArgs = []
    FscFiles = []
    FsiArgs = []
    FsiFiles = []
    UseFiles = []
    OutArgs = []
    OutFiles = []
    EtcArgs = []
    EtcFiles = []
}

type private ConfigSection =
    | NoSection
    | FscSection
    | FsiSection
    | OutSection
    | EtcSection
    | UseSection

type private ConfigLine =
    | EmptyLine
    | CommentLine
    | SectionLine of string
    | SwitchLine of string
    | ValueLine of string
    | KeyValueLine of Key : string * Value : string

let private parseLine (line: string) =
    let line = line.Trim ()
    if line.Length = 0 then
        EmptyLine
    elif line.[0] = ';' then
        CommentLine
    elif line.[0] = '[' then
        if not (line.EndsWith "]") then
            invalidOp "Invalid section, expected '[...]'."
        SectionLine (line.Substring(1, line.Length - 2).Trim ())
    elif line.[0] <> '-' then
        ValueLine line
    else
        let i = line.IndexOf ':'
        if i < 0 then
            SwitchLine line
        else
            KeyValueLine (line.Substring(0, i).TrimEnd (), line.Substring(i + 1).TrimStart ())

// Expands variables and for some keys resolves full paths.
let private resolveKeyValue root key value =
    let value = Environment.ExpandEnvironmentVariables(value).Replace ("__SOURCE_DIRECTORY__", root)
    match key with
    | "-r" | "--reference" ->
        // resolve a path only if it starts with "." else keep it, e.g. `-r:System.Management.Automation`
        if value.[0] = '.' then
            Path.GetFullPath (Path.Combine (root, value))
        else
            value
    | "" | "-l" | "--lib" | "-o" | "--out" | "--use" | "--doc" ->
        if Path.IsPathRooted value then
            Path.GetFullPath value
        else
            Path.GetFullPath (Path.Combine (root, value))
    | _ ->
        value

// Reads configuration from the specified file avoiding recursive loops and duplicated configurations.
// Duplicates look fine for the compiler but VS is not happy with same items in generated projects.
let rec private readConfigFromFileRec path parents : Config =
    let lines = File.ReadAllLines path
    let root = Path.GetDirectoryName path

    let mutable fscArgs = []
    let mutable fscFiles = []
    let mutable fsiArgs = []
    let mutable fsiFiles = []
    let mutable useFiles = []
    let mutable outArgs = []
    let mutable outFiles = []
    let mutable etcArgs = []
    let mutable etcFiles = []

    let mutable currentSection = NoSection
    let mutable lineNo = 0

    let raiseSection () = invalidOp "Expected section [fsc|fsi|out|etc|use], found data or unknown section."
    try
        for line in lines do
            lineNo <- lineNo + 1
            match parseLine line with
            | EmptyLine
            | CommentLine ->
                ()
            | SectionLine section ->
                currentSection <-
                    match section with
                    | "fsc" -> FscSection
                    | "fsi" -> FsiSection
                    | "out" -> OutSection
                    | "etc" -> EtcSection
                    | "use" -> UseSection
                    | _ -> raiseSection ()
            | SwitchLine it ->
                match currentSection with
                | FscSection ->
                    fscArgs <- it :: fscArgs
                | FsiSection ->
                    fsiArgs <- it :: fsiArgs
                | OutSection ->
                    outArgs <- it :: outArgs
                | EtcSection ->
                    etcArgs <- it :: etcArgs
                | UseSection
                | NoSection ->
                    raiseSection ()
            | ValueLine it ->
                let file = resolveKeyValue root "" it
                match currentSection with
                | FscSection ->
                    fscFiles <- file :: fscFiles
                | FsiSection ->
                    fsiFiles <- file :: fsiFiles
                | OutSection ->
                    outFiles <- file :: outFiles
                | EtcSection ->
                    etcFiles <- file :: etcFiles
                | UseSection ->
                    if not (Seq.containsIgnoreCase file !parents) then
                        parents := file :: !parents
                        let config = readConfigFromFileRec file parents
                        fscArgs <- config.FscArgs @ fscArgs 
                        fscFiles <- config.FscFiles @ fscFiles
                        fsiArgs <- config.FsiArgs @ fsiArgs
                        fsiFiles <- config.FsiFiles @ fsiFiles
                        useFiles <- config.UseFiles @ useFiles
                        outArgs <- config.OutArgs @ outArgs
                        outFiles <- config.OutFiles @ outFiles
                        etcArgs <- config.EtcArgs @ etcArgs
                        etcFiles <- config.EtcFiles @ etcFiles
                | NoSection ->
                    raiseSection ()
            | KeyValueLine (key, value) ->
                let text = resolveKeyValue root key value
                match currentSection with
                | FscSection ->
                    // use -r instead of --reference to avoid duplicates added by FCS
                    // https://github.com/fsharp/FSharp.Compiler.Service/issues/697
                    let key = if key = "--reference" then "-r" else key
                    fscArgs <- (key + ":" + text) :: fscArgs
                | FsiSection ->
                    if key = "--use" then
                        useFiles <- text :: useFiles
                    else
                        fsiArgs <- (key + ":" + text) :: fsiArgs
                | OutSection ->
                    outArgs <- (key + ":" + text) :: outArgs
                | EtcSection ->
                    etcArgs <- (key + ":" + text) :: etcArgs
                | UseSection
                | NoSection ->
                    raiseSection ()
     with e ->
        invalidOp (sprintf "%s(%d): %s" path lineNo e.Message)

    {
        FscArgs = List.rev fscArgs
        FscFiles = List.rev fscFiles
        FsiArgs = List.rev fsiArgs
        FsiFiles = List.rev fsiFiles
        UseFiles = List.rev useFiles
        OutArgs = List.rev outArgs
        OutFiles = List.rev outFiles
        EtcArgs = List.rev etcArgs
        EtcFiles = List.rev etcFiles
    }

/// Reads the config from the specified file.
let readConfigFromFile path =
    readConfigFromFileRec path (ref [ path ])

/// Tries to get the config file path in the specified directory.
/// If there are many then the first in alphabetical order is used.
let tryConfigPathInDirectory dir =
    let files = Directory.GetFiles (dir, "*.fs.ini")
    match files.Length with
    | 1 ->
        Some files.[0]
    | 0 ->
        None
    | _ ->
        Array.Sort (files, StringComparer.OrdinalIgnoreCase)
        Some files.[0]

/// Get the local or main config file in the specified directory.
let getConfigPathInDirectory dir =
    match tryConfigPathInDirectory dir with
    | Some file ->
        // local config
        file
    | None ->
        // main config
        farMainConfigPath

/// Gets the local or main config path for the file.
let getConfigPathForFile path =
    getConfigPathInDirectory (Path.GetDirectoryName path)

/// Gets the local or main config for the file.
let getConfigForFile path =
    readConfigFromFile (getConfigPathForFile path)

/// Makes the temp project for the specified config file.
let generateProject configPath =
    let configRoot = Path.GetDirectoryName configPath
    let configRootName = Path.GetFileName configRoot
    let configBaseName =
        let name = Path.GetFileNameWithoutExtension configPath
        if String.equalsIgnoreCase name ".fs" then configRootName + ".fs" else name
    let nameInTemp = sprintf "_Project-%s-%08x" configRootName ((configRoot.ToUpper ()).GetHashCode ())
    let projectRoot = Path.Combine (Path.GetTempPath (), nameInTemp)
    let projectPath = Path.Combine (projectRoot, configBaseName + ".fsproj")
    Directory.CreateDirectory projectRoot |> ignore

    let config = readConfigFromFile configPath

    let xml = XmlDocument ()
    xml.InnerXml <- """<Project Sdk="Microsoft.NET.Sdk"/>"""
    let doc = xml.DocumentElement
    let nodeProperties = doc.AppendChild (xml.CreateElement "PropertyGroup")
    let nodeItems = doc.AppendChild (xml.CreateElement "ItemGroup")

    let addProperty name value =
        (nodeProperties.AppendChild (xml.CreateElement name)).InnerText <- value

    let addReference reference =
        let node = xml.CreateElement "Reference"
        nodeItems.AppendChild node |> ignore
        node.SetAttribute ("Include", Path.GetFileNameWithoutExtension reference)
        (node.AppendChild (xml.CreateElement "HintPath")).InnerText <- reference
        (node.AppendChild (xml.CreateElement "Private")).InnerText <- "false"

    let addFile file =
        let node = xml.CreateElement "Compile"
        nodeItems.AppendChild node |> ignore
        node.SetAttribute ("Include", file)

    addProperty "TargetFramework" "net462"
    addProperty "DisableImplicitFSharpCoreReference" "true"
    addProperty "DisableImplicitSystemValueTupleReference" "true"

    let dir = Environment.GetEnvironmentVariable "FARHOME"
    addReference (dir + @"\FSharp.Core.dll")
    addReference (dir + @"\FarNet\FarNet.dll")
    addReference (dir + @"\FarNet\FarNet.FSharp.dll")
    addReference (dir + @"\FarNet\FarNet.Tools.dll")
    addReference (dir + @"\FarNet\Modules\FSharpFar\FSharpFar.dll")

    for op in config.FscArgs do
        if op.StartsWith "-r:" then
            addReference (op.Substring 3)
        elif op.StartsWith "-I:" then
            addProperty "ReferencePath" (op.Substring 3)
        elif op.StartsWith "--lib:" then
            addProperty "ReferencePath" (op.Substring 6)

    for file in config.FscFiles do
        addFile file

    for file in Directory.EnumerateFiles (configRoot, "*.fs") do
        if not (Seq.containsIgnoreCase file config.FscFiles) then
            addFile file

    xml.Save projectPath
    projectPath
