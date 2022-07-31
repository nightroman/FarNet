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
    let defaultFileForFile (path: string) =
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
        "type": "coreclr",
        "request": "launch",
        "externalConsole": true,
        "program": "$FAR"
      },
      {
        "name": "Attach Far",
        "type": "coreclr",
        "request": "attach",
        "processName": "Far"
      },
      {
        "name": "Attach process",
        "type": "coreclr",
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
        let dir2 = Directory.CreateDirectory(Path.Combine(dir, ".vscode"))
        File.WriteAllText(Path.Combine(dir2.FullName, "settings.json"), textVSCodeSettings ())

    /// Makes the temp project for the specified config file.
    let generateProject (configPath: string) =
        // get .fsproj path and make its folder
        let projectPath =
            let projectName, folderName =
                let projectName = configPath |> Path.GetFileNameWithoutExtension |> Path.GetFileNameWithoutExtension
                let projectName, folderPath =
                    let folderPath = configPath |> Path.GetDirectoryName
                    if projectName.Length = 0 then
                        (folderPath |> Path.GetFileName, folderPath)
                    else
                        (projectName, folderPath |> Path.GetDirectoryName)
                let folderName =
                    let folderName = folderPath |> Path.GetFileName
                    if String.equalsIgnoreCase folderName projectName then
                        folderPath |> Path.GetDirectoryName |> Path.GetFileName
                    else
                        folderName
                (projectName, folderName)
            let nameInTemp = "_Project_" + projectName + "_" + folderName
            let projectRoot = Path.Combine(Path.GetTempPath(), nameInTemp)
            Directory.CreateDirectory projectRoot |> ignore
            projectRoot + "\\" + projectName + ".fsproj"

        // read config
        let config = Config.readFromFile configPath

        // merge with defaults
        let config = { config with FscArgs = Array.append defaultCompilerArgs config.FscArgs }

        let xml = XmlDocument()
        xml.InnerXml <- """<Project Sdk="Microsoft.NET.Sdk"/>"""
        let doc = xml.DocumentElement
        let nodeProperties = doc.AppendChild(xml.CreateElement "PropertyGroup")
        let nodeItems = doc.AppendChild(xml.CreateElement "ItemGroup")

        let addProperty name value =
            (nodeProperties.AppendChild(xml.CreateElement name)).InnerText <- value

        let addReference reference =
            if reference = "System.Windows.Forms" then ()
            else
            let node = xml.CreateElement "Reference"
            nodeItems.AppendChild node |> ignore
            if File.Exists reference then
                node.SetAttribute("Include", Path.GetFileNameWithoutExtension reference)
                (node.AppendChild(xml.CreateElement "HintPath")).InnerText <- reference
                (node.AppendChild(xml.CreateElement "Private")).InnerText <- "false"
            else
                // -r:System.Management.Automation
                node.SetAttribute("Include", reference)

        let addFile file =
            let node = xml.CreateElement "Compile"
            nodeItems.AppendChild node |> ignore
            node.SetAttribute("Include", file)

        addProperty "StartAction" "Program"
        addProperty "StartProgram" (getFarExePath ())

        //! use `windows` or charting is not happy
        addProperty "TargetFramework" "net6.0-windows"
        addProperty "UseWindowsForms" "true"
        addProperty "DisableImplicitFSharpCoreReference" "true"

        // https://github.com/dotnet/sdk/issues/987
        // Works just for VS and MSBuild. Well, at least VS is happy.
        addProperty "AssemblySearchPaths" "$(AssemblySearchPaths);{GAC}"

        // respect output
        config.OutArgs
        |> Array.tryPick (fun x ->
            if x.StartsWith("--out:") then
                Some x[6..]
            else if x.StartsWith("-o:") then
                Some x[3..]
            else
                None
        )
        |> function
        | None ->
            ()
        | Some output ->
            addProperty "OutDir" (Path.GetDirectoryName(output))
            addProperty "AssemblyName" (Path.GetFileNameWithoutExtension(output))

        do
            let flags = ResizeArray()

            for op in config.FscArgs do
                if op.StartsWith "-r:" then
                    addReference (op.Substring 3)
                elif op.StartsWith "-I:" then
                    addProperty "ReferencePath" (op.Substring 3)
                elif op.StartsWith "--lib:" then
                    addProperty "ReferencePath" (op.Substring 6)
                else
                    flags.Add op

            addProperty "OtherFlags" (String.Join(" ", flags))

        for file in config.FscFiles do
            addFile file

        let configRoot = Path.GetDirectoryName configPath

        for file in Directory.EnumerateFiles(configRoot, "*.fs") do
            if not (Seq.containsIgnoreCase file config.FscFiles) then
                addFile file

        for file in Directory.EnumerateFiles(configRoot, "*.fsx") do
            if not (Seq.containsIgnoreCase file config.FscFiles) then
                addFile file

        xml.Save projectPath
        projectPath
