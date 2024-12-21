module AssemblyResolver

open System
open System.Diagnostics
open System.IO
open System.Collections.Generic
open System.Reflection
open System.Runtime.InteropServices

let [<Literal>] private MaxLastRoots = 4
let [<Literal>] private Win64 = "win-x64"
let [<Literal>] private Win86 = "win-x86"

let mutable private _folders : string[] = Array.empty
let private _roots = LinkedList<string>()
let private _win_this = RuntimeInformation.RuntimeIdentifier
let private _win_skip = match _win_this with Win64 -> Win86 | Win86 -> Win64 | _ -> failwith "Unknown runtime."

let init (folders : string seq) =
    _folders <- Seq.toArray folders

let private isRoot (name: string) =
    (name.Contains("\\FarNet\\Modules") || name.Contains("\\FarNet\\Lib")) && not (name.Contains("\\runtimes"))

let private addRoot (name: string) =
    if _roots.Count > 0 && _roots.First.Value = name then ()
    else

    _roots.Remove(name) |> ignore
    _roots.AddFirst(name) |> ignore
    while _roots.Count > MaxLastRoots do
        _roots.RemoveLast()

let resolvePowerShellFar (root: string) (dllName: string) (callerFullName: string) =
    // most frequent
    // System.Management.Automation ->
    //   Microsoft.PowerShell.ConsoleHost
    //   Microsoft.PowerShell.Commands.Utility
    //   Microsoft.PowerShell.Commands.Management
    //   Microsoft.PowerShell.Security
    let assembly =
        if callerFullName.StartsWith("System.Management.Automation") then
            let path = $"{root}\\runtimes\\win\\lib\\net9.0\\{dllName}"
            if File.Exists(path) then
                Assembly.LoadFrom(path)
            else
                null
        else
            null

    if not (isNull assembly) then assembly
    else

    let assembly =
        if callerFullName.StartsWith("System.Management.Automation") then
            // System.Management.Automation ->
            //   System.Management
            let path = $"{root}\\{dllName}"
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
    let win_x = RuntimeInformation.RuntimeIdentifier
    let path = $"{root}\\runtimes\\{win_x}\\lib\\netstandard1.6\\{dllName}"
    if File.Exists(path) then
        Assembly.LoadFrom(path)
    else
        null

let assemblyResolve _ (args: ResolveEventArgs) =
    // e.g. XmlSerializers
    if isNull args.RequestingAssembly then null
    else

    // skip .resources
    // some exist, usually several files in different folders, so not useful for us
    // some do not, e.g. frequently called System.Management.Automation.resources
    let name = args.Name.Substring(0, args.Name.IndexOf(','))
    if name.EndsWith(".resources") then null
    else

    // skip missing in folders
    let dllName = $"{name}.dll"
    let paths = ResizeArray(_folders |> Seq.collect (fun x -> Directory.EnumerateFiles(x, dllName, SearchOption.AllDirectories)))

    if paths.Count = 0 then null
    else

    Debug.WriteLine($"## assemblyResolve {name}")

    if paths.Count > 1 then
        paths.RemoveAll(fun x -> x.Contains(_win_skip)) |> ignore

    // one in folders
    if paths.Count = 1 then
        Assembly.LoadFrom(paths[0])
    else

    let assembly =
        if args.RequestingAssembly.IsDynamic then
            null
        else
            let callerLocation = args.RequestingAssembly.Location

            // case: PowerShellFar
            let index = callerLocation.LastIndexOf("\\PowerShellFar\\")
            if index > 0 then
                resolvePowerShellFar (callerLocation.Substring(0, index + 14)) dllName args.RequestingAssembly.FullName
            else
                // case: same folder as the caller
                let callerRoot = Path.GetDirectoryName(callerLocation)
                let path = callerRoot + "\\" + dllName
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
        let path = root + "\\" + dllName;
        if File.Exists(path) then
            addRoot root
            Some (Assembly.LoadFrom(path))
        else
            None
    )
    |> Option.toObj
