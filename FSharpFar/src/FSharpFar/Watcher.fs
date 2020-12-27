module FSharpFar.Watcher
open FarNet.FSharp
open System
open System.IO
open System.Collections.Generic

let private agent = MailboxProcessor.Start (fun inbox -> async {
    while true do
        let! path = inbox.Receive ()
        if isFSharpFileName path then
            do! Jobs.Job <| fun () ->
                Session.OnSavingSource path
        else if String.endsWithIgnoreCase path ".fs.ini" then
            do! Jobs.Job <| fun () ->
                Session.OnSavingConfig path
})

let private watchers = Dictionary (StringComparer.OrdinalIgnoreCase)

let add directoryPath =
    if not (watchers.ContainsKey directoryPath) then
        let watcher = new FileSystemWatcher ()
        watcher.Path <- directoryPath
        watcher.Created.Add (fun e -> agent.Post e.FullPath)
        watcher.Changed.Add (fun e -> agent.Post e.FullPath)
        watcher.Deleted.Add (fun e -> agent.Post e.FullPath)
        watcher.Renamed.Add (fun e -> agent.Post e.OldFullPath)
        watcher.SynchronizingObject <- null
        watcher.EnableRaisingEvents <- true
        watchers.Add (directoryPath, watcher)
