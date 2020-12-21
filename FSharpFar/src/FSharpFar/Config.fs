namespace FSharpFar
open System
open System.IO
open System.Xml

[<RequireQualifiedAccess>]
module Config =
    /// Get the local or main config file for the specified directory.
    let defaultFileForDirectory dir =
        match Config.tryFindFileInDirectory dir with
        | Some file ->
            // local config
            file
        | None ->
            // main config
            farMainConfigPath

    /// Get the local or main config file for the current directory.
    let defaultFile () =
        defaultFileForDirectory (farCurrentDirectory ())

    /// Gets the local or main config path for the file.
    let defaultFileForFile path =
        defaultFileForDirectory (Path.GetDirectoryName path)

    /// Reads the config for the file.
    let readForFile path =
        Config.readFromFile (defaultFileForFile path)

    let private getFarExePath () =
        Path.Combine(Environment.GetEnvironmentVariable "FARHOME", "Far.exe")

    let private textVSCodeSettings () =
        let template = """{
  "launch": {
    "version": "0.2.0",
    "configurations": [
      {
        "name": "Start Far",
        "type": "clr",
        "request": "launch",
        "externalConsole": true,
        "program": "$FAR"
      },
      {
        "name": "Attach Far",
        "type": "clr",
        "request": "attach",
        "processName": "Far"
      },
      {
        "name": "Attach process",
        "type": "clr",
        "request": "attach",
        "processId": "${command:pickProcess}"
      }
    ]
  }
}"""
        template.Replace(
            "$FAR",
            getFarExePath().Replace(@"\", @"\\")
        )

    /// Writes VSCode settings.json
    let writeVSCodeSettings dir =
        let dir2 = Directory.CreateDirectory (Path.Combine (dir, ".vscode"))
        File.WriteAllText (Path.Combine (dir2.FullName, "settings.json"), textVSCodeSettings ())

    /// Makes the temp project for the specified config file.
    let generateProject configPath =
        let configRoot = Path.GetDirectoryName configPath
        let projectName =
            let name = Path.GetFileNameWithoutExtension configPath
            if String.equalsIgnoreCase name ".fs" then (Path.GetFileName configRoot) + ".fs" else name
        let nameInTemp = sprintf "_Project-%s-%08x" projectName ((configPath.ToUpper ()).GetHashCode ())
        let projectRoot = Path.Combine (Path.GetTempPath (), nameInTemp)
        let projectPath = Path.Combine (projectRoot, projectName + ".fsproj")
        Directory.CreateDirectory projectRoot |> ignore

        // read config
        let config = Config.readFromFile configPath

        // merge with defaults
        let config = { config with FscArgs = Array.append defaultCompilerArgs config.FscArgs }

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
            if File.Exists reference then
                node.SetAttribute ("Include", Path.GetFileNameWithoutExtension reference)
                (node.AppendChild (xml.CreateElement "HintPath")).InnerText <- reference
                (node.AppendChild (xml.CreateElement "Private")).InnerText <- "false"
            else
                // -r:System.Management.Automation
                node.SetAttribute ("Include", reference)

        let addFile file =
            let node = xml.CreateElement "Compile"
            nodeItems.AppendChild node |> ignore
            node.SetAttribute ("Include", file)

        addProperty "StartAction" "Program"
        addProperty "StartProgram" (getFarExePath ())
        addProperty "TargetFramework" "net472"
        addProperty "DisableImplicitFSharpCoreReference" "true"
        addProperty "DisableImplicitSystemValueTupleReference" "true"
        // https://github.com/dotnet/sdk/issues/987
        // Works just for VS and MSBuild. Well, at least VS is happy.
        addProperty "AssemblySearchPaths" "$(AssemblySearchPaths);{GAC}"

        do
            let flags = ResizeArray ()

            for op in config.FscArgs do
                if op.StartsWith "-r:" then
                    addReference (op.Substring 3)
                elif op.StartsWith "-I:" then
                    addProperty "ReferencePath" (op.Substring 3)
                elif op.StartsWith "--lib:" then
                    addProperty "ReferencePath" (op.Substring 6)
                else
                    flags.Add op

            addProperty "OtherFlags" (String.Join (" ", flags))

        for file in config.FscFiles do
            addFile file

        for file in Directory.EnumerateFiles (configRoot, "*.fs") do
            if not (Seq.containsIgnoreCase file config.FscFiles) then
                addFile file

        for file in Directory.EnumerateFiles (configRoot, "*.fsx") do
            if not (Seq.containsIgnoreCase file config.FscFiles) then
                addFile file

        xml.Save projectPath
        projectPath
