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

type private ConfigLine =
    | Empty
    | Comment
    | Section of string
    | Switch of string
    | Value of string
    | Pair of Key : string * Value : string

let private parse (line: string) =
    let text = line.Trim ()
    if text.Length = 0 then
        Empty
    elif text.[0] = ';' then
        Comment
    elif text.[0] = '[' then
        if not (text.EndsWith "]") then
            invalidOp "Invalid section, expected '[...]'."
        Section (text.Substring(1, text.Length - 2).Trim ())
    elif text.[0] <> '-' then
        Value text
    else
        let i = text.IndexOf ':'
        if i < 0 then
            Switch text
        else
            Pair (text.Substring(0, i).Trim (), text.Substring(i + 1).Trim ())

let private resolve root key value =
    let value = Environment.ExpandEnvironmentVariables(value).Replace ("__SOURCE_DIRECTORY__", root)
    match key with
    | "-r" | "--reference" ->
        // resolve a path only if it starts with "." else use as it is, e.g. `-r:System.Management.Automation`
        if value.[0] = '.' then
            Path.GetFullPath (Path.Combine(root, value))
        else
            value
    | "" | "-l" | "--lib" | "-o" | "--out" | "--use" | "--doc" ->
        if Path.IsPathRooted value then
            Path.GetFullPath value
        else
            Path.GetFullPath (Path.Combine(root, value))
    | _ ->
        value

let readConfigFromFile path =
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

    let raiseSection () = invalidOp "Expected section [fsc|fsi|out|etc], found data or unknown section."
    try
        for line in lines do
            lineNo <- lineNo + 1
            match parse line with
            | Empty | Comment ->
                ()
            | Section section ->
                currentSection <-
                    match section with
                    | "fsc" -> FscSection
                    | "fsi" -> FsiSection
                    | "out" -> OutSection
                    | "etc" -> EtcSection
                    | _ -> raiseSection ()
            | Switch it ->
                match currentSection with
                | FscSection ->
                    fscArgs <- it :: fscArgs
                | FsiSection ->
                    fsiArgs <- it :: fsiArgs
                | OutSection ->
                    outArgs <- it :: outArgs
                | EtcSection ->
                    etcArgs <- it :: etcArgs
                | NoSection ->
                    raiseSection ()
            | Value it ->
                let file = resolve root "" it
                match currentSection with
                | FscSection ->
                    fscFiles <- file :: fscFiles
                | FsiSection ->
                    fsiFiles <- file :: fsiFiles
                | OutSection ->
                    outFiles <- file :: outFiles
                | EtcSection ->
                    etcFiles <- file :: etcFiles
                | NoSection ->
                    raiseSection ()
            | Pair (key, value) ->
                let text = resolve root key value
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

/// Gets and caches the config from a file.
let getConfigFromFileCached =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<string, DateTime * Config> StringComparer.OrdinalIgnoreCase
    fun path ->
        let time1 = File.GetLastWriteTime path
        let add path = time1, readConfigFromFile path
        let update path ((time2, _) as value) = if time1 = time2 then value else add path
        let _, config = cache.AddOrUpdate (path, add, update)
        config

/// Gets some config path in a directory.
let tryConfigPathInDirectory dir =
    match Directory.GetFiles (dir, "*.fs.ini") with
    | [|file|] ->
        Some file
    | _ ->
        None

/// Gets the local or main config path for a file.
let getConfigPathForFile path =
    let dir = Path.GetDirectoryName path
    match tryConfigPathInDirectory dir with
    // local config
    | Some file ->
        file
    // main config
    | _ ->
        farMainConfigPath

/// Gets the local or main config for a file.
let getConfigForFile path =
    getConfigFromFileCached (getConfigPathForFile path)

let generateProject path =
    let fileName = Path.GetFileNameWithoutExtension path
    let fileRoot = Path.GetDirectoryName path
    let nameInTemp = sprintf "FS-%s-%08X" (Path.GetFileName fileRoot) ((fileRoot.ToUpper ()).GetHashCode ())
    let projectRoot = Path.Combine (Path.GetTempPath (), nameInTemp)
    let projectPath = Path.Combine (projectRoot, fileName + ".fsproj")
    Directory.CreateDirectory projectRoot |> ignore

    let config = readConfigFromFile path

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

    for file in Directory.EnumerateFiles(fileRoot, "*.fs") do
        if not (Seq.containsIgnoreCase file config.FscFiles) then
            addFile file

    xml.Save projectPath
    projectPath
