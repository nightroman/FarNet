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
        paths.RemoveAll(fun x -> x.Contains(Win86)) |> ignore

    // one in folders
    if paths.Count = 1 then
        Assembly.LoadFrom(paths[0])
    else

    let assembly =
        if args.RequestingAssembly.IsDynamic then
            null
        else
            let callerLocation = args.RequestingAssembly.Location

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
