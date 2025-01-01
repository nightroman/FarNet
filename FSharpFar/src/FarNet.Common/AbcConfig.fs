[<AutoOpen>]
module FSharpFar.AbcConfig
open System
open System.IO

/// Configuration data for checkers and sessions.
type Config = {
    FscArgs: string []
    FscFiles: string []
    FsiArgs: string []
    FsiFiles: string []
    UseFiles: string []
    OutArgs: string []
    OutFiles: string []
    EtcArgs: string []
    EtcFiles: string []
}

[<RequireQualifiedAccess>]
module Config =
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
        let line = line.Trim()
        if line.Length = 0 then
            EmptyLine
        elif line[0] = ';' then
            CommentLine
        elif line[0] = '[' then
            if not (line.EndsWith "]") then
                invalidOp "Invalid section, expected '[...]'."
            SectionLine(line.Substring(1, line.Length - 2).Trim())
        elif line[0] <> '-' then
            ValueLine line
        else
            let i = line.IndexOf ':'
            if i < 0 then
                SwitchLine line
            else
                KeyValueLine(line.Substring(0, i).TrimEnd(), line.Substring(i + 1).TrimStart())

    // Used by FarNet.FSharp.Charting.ini
    let [<Literal>] private EnvironmentVersion = "%$Version%"

    // Expands variables and resolves full paths.
    let private resolveKeyValue (root: string) (key: string) (value: string) =
        let value = if value.Contains(EnvironmentVersion) then value.Replace(EnvironmentVersion, Environment.Version.ToString()) else value
        let value = Environment.ExpandEnvironmentVariables(value).Replace("__SOURCE_DIRECTORY__", root)
        match key with
        | "-r" | "--reference" ->
            // resolve a path only if it starts with "." else keep it, e.g. `-r:System.Management.Automation`
            if value[0] = '.' then
                Path.GetFullPath(Path.Combine(root, value))
            else
                value
        | "" | "-l" | "--lib" | "-o" | "--out" | "--use" | "--doc" ->
            if Path.IsPathRooted value then
                Path.GetFullPath value
            else
                Path.GetFullPath(Path.Combine(root, value))
        | _ ->
            value

    // Reads configuration from the specified file avoiding recursive loops and duplicated configurations.
    // Duplicates look fine for the compiler but VS is not happy with same items in generated projects.
    let rec private readConfigFromFileRec path (parents: string list ref) : Config =
        let lines = File.ReadAllLines path
        let root = Path.GetDirectoryName path

        let fscArgs = ResizeArray()
        let fscFiles = ResizeArray()
        let fsiArgs = ResizeArray()
        let fsiFiles = ResizeArray()
        let useFiles = ResizeArray()
        let outArgs = ResizeArray()
        let outFiles = ResizeArray()
        let etcArgs = ResizeArray()
        let etcFiles = ResizeArray()

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
                        fscArgs.Add it
                    | FsiSection ->
                        fsiArgs.Add it
                    | OutSection ->
                        outArgs.Add it
                    | EtcSection ->
                        etcArgs.Add it
                    | UseSection
                    | NoSection ->
                        raiseSection ()
                | ValueLine it ->
                    let file = resolveKeyValue root "" it
                    match currentSection with
                    | FscSection ->
                        fscFiles.Add file
                    | FsiSection ->
                        fsiFiles.Add file
                    | OutSection ->
                        outFiles.Add file
                    | EtcSection ->
                        etcFiles.Add file
                    | UseSection ->
                        if not (parents.Value |> Seq.exists (fun x -> file.Equals(x, StringComparison.OrdinalIgnoreCase))) then
                            parents.Value <- file :: parents.Value
                            let config = readConfigFromFileRec file parents
                            fscArgs.AddRange config.FscArgs
                            fscFiles.AddRange config.FscFiles
                            fsiArgs.AddRange config.FsiArgs
                            fsiFiles.AddRange config.FsiFiles
                            useFiles.AddRange config.UseFiles
                            outArgs.AddRange config.OutArgs
                            outFiles.AddRange config.OutFiles
                            etcArgs.AddRange config.EtcArgs
                            etcFiles.AddRange config.EtcFiles
                    | NoSection ->
                        raiseSection ()
                | KeyValueLine(key, value) ->
                    let text = resolveKeyValue root key value
                    match currentSection with
                    | FscSection ->
                        // use -r instead of --reference to avoid duplicates added by FCS
                        // https://github.com/fsharp/FSharp.Compiler.Service/issues/697
                        let key = if key = "--reference" then "-r" else key
                        fscArgs.Add(key + ":" + text)
                    | FsiSection ->
                        if key = "--use" then
                            useFiles.Add text
                        else
                            fsiArgs.Add(key + ":" + text)
                    | OutSection ->
                        outArgs.Add(key + ":" + text)
                    | EtcSection ->
                        etcArgs.Add(key + ":" + text)
                    | UseSection
                    | NoSection ->
                        raiseSection ()
         with e ->
            invalidOp $"{path}({lineNo}): {e.Message}"

        {
            FscArgs = fscArgs.ToArray()
            FscFiles = fscFiles.ToArray()
            FsiArgs = fsiArgs.ToArray()
            FsiFiles = fsiFiles.ToArray()
            UseFiles = useFiles.ToArray()
            OutArgs = outArgs.ToArray()
            OutFiles = outFiles.ToArray()
            EtcArgs = etcArgs.ToArray()
            EtcFiles = etcFiles.ToArray()
        }

    /// Reads the config from the specified file.
    let readFromFile path =
        readConfigFromFileRec path (ref [ path ])

    /// Tries to get the config file path in the specified directory.
    /// If there are many then the first in alphabetical order is used.
    let tryFindFileInDirectory dir =
        let files = Directory.GetFiles(dir, "*.fs.ini")
        match files.Length with
        | 1 ->
            Some files[0]
        | 0 ->
            None
        | _ ->
            Array.Sort(files, StringComparer.OrdinalIgnoreCase)
            Some files[0]
