
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FsfModuleCommand

open FarNet
open Session
open System

/// Shows an exception.
#if _
let showError exn =
    Far.Api.ShowError("Error", exn)
#else
let showError exn =
    Far.Api.UI.WriteLine(sprintf "%A" exn, ConsoleColor.Red)
#endif

[<System.Runtime.InteropServices.Guid("2b52615b-ea79-46e4-ac9d-78f33599db62")>]
[<ModuleCommand(Name = "FSharpFar", Prefix = "fs")>]
type FsfModuleCommand() =
    inherit ModuleCommand()
    override x.Invoke(sender, e) =
        let session = getMainSession()
        let cd = Environment.CurrentDirectory
        try
            Far.Api.UI.ShowUserScreen()
            Far.Api.UI.WriteLine((sprintf "fs:%s" e.Command), ConsoleColor.DarkGray)
            Environment.CurrentDirectory <- Far.Api.Panel.CurrentDirectory
            let r = session.Invoke Console.Out e.Command
            for w in r.Warnings do
                Far.Api.UI.WriteLine(formatFSharpErrorInfo w, ConsoleColor.Yellow)
            if r.Exception <> null then
                showError r.Exception
        finally
            Far.Api.UI.SaveUserScreen()
            Environment.CurrentDirectory <- cd
