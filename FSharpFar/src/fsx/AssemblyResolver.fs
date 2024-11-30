module AssemblyResolver

open System
open System.IO
open System.Collections.Generic
open System.Reflection

let private _cache = Dictionary<string, obj>(StringComparer.OrdinalIgnoreCase)
let private _roots = LinkedList<string>()

let prepare root =
    for path in Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories) do
        if path.Contains("\\native\\") then ()
        else
        let key = Path.GetFileNameWithoutExtension(path)

        // add, if 2+ null it
        if not (_cache.TryAdd(key, path)) then
            _cache[key] <- null

let private assemblyNameToDllName (name: string) =
    name.Substring(0, name.IndexOf(',')) + ".dll"

let private isRoot (name: string) =
    (name.Contains("\\FarNet\\Modules") || name.Contains("\\FarNet\\Lib")) && not (name.Contains("\\runtimes"))

let private addRoot (name: string) =
    if _roots.Count > 0 && _roots.First.Value = name then ()
    else

    _roots.Remove(name) |> ignore
    _roots.AddFirst(name) |> ignore
    while _roots.Count > 4 do
        _roots.RemoveLast()

let resolvePowerShellFar (root: string) (args: ResolveEventArgs) =
    let caller = args.RequestingAssembly.FullName
    let dllName = assemblyNameToDllName args.Name

    // most frequent
    // System.Management.Automation ->
    //   Microsoft.PowerShell.ConsoleHost
    //   Microsoft.PowerShell.Commands.Utility
    //   Microsoft.PowerShell.Commands.Management
    //   Microsoft.PowerShell.Security
    let assembly =
        if caller.StartsWith("System.Management.Automation") then
            let path = root + "\\runtimes\\win\\lib\\net9.0\\" + dllName
            if File.Exists(path) then
                Assembly.LoadFrom(path)
            else
                null
        else
            null

    if not (isNull assembly) then assembly
    else

    let assembly =
        if caller.StartsWith("System.Management.Automation") then
            // System.Management.Automation ->
            //   System.Management
            let path = root + "\\" + dllName
            if File.Exists(path) then
                Assembly.LoadFrom(path)
            else
                null
        else
            null

    if not (isNull assembly) then assembly
    else

    // Microsoft.PowerShell.Commands.Management ->
    // System.Management.Automation ->
    //   Microsoft.Management.Infrastructure
    let win10_x64 = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
    let path = root + "\\runtimes\\" + win10_x64 + "\\lib\\netstandard1.6\\" + dllName
    if File.Exists(path) then
        Assembly.LoadFrom(path)
    else
        null

let assemblyResolve _ (args: ResolveEventArgs) =
    // e.g. .XmlSerializers
    if isNull args.RequestingAssembly then null
    else

    // skip .resources
    // some exist, usually several files in different folders, so not useful for us
    // some do not, e.g. frequently called System.Management.Automation.resources
    let name = args.Name.Substring(0, args.Name.IndexOf(','))
    if name.EndsWith(".resources") then null
    else

    // skip missing in cache
    match _cache.TryGetValue(name) with
    | false, _ -> null
    | _, value ->

    System.Diagnostics.Debug.WriteLine($"fsx: {name}")

    // unique in FarNet, load once
    if not (isNull value) then
        match value with
        | :? string as path ->
            let assembly = Assembly.LoadFrom(path)
            _cache[name] <- assembly
            assembly
        | _ ->
            let assembly = value :?> Assembly
            let location = assembly.Location
            if isRoot location then
                addRoot (Path.GetDirectoryName(location))
            assembly
    else

    let dllName = lazy (assemblyNameToDllName args.Name)
    let assembly =
        if args.RequestingAssembly.IsDynamic then
            null
        else
            let callerFile = args.RequestingAssembly.Location

            // case: PowerShellFar
            let index = callerFile.LastIndexOf("\\PowerShellFar\\")
            if index > 0 then
                resolvePowerShellFar (callerFile.Substring(0, index + 14)) args
            else
                // case: same folder as the caller
                let callerRoot = Path.GetDirectoryName(callerFile)
                let path = callerRoot + "\\" + dllName.Value
                if File.Exists(path) then
                    if isRoot callerRoot then
                        addRoot callerRoot
                    Assembly.LoadFrom(path)
                else
                    null

    if not (isNull assembly) then assembly
    else

    // case: same folder as last roots
    _roots
    |> Seq.tryPick (fun root ->
        let path = root + "\\" + dllName.Value;
        if File.Exists(path) then
            addRoot root
            Some (Assembly.LoadFrom(path))
        else
            None
    )
    |> Option.toObj
