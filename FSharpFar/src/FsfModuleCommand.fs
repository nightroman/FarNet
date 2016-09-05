
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

namespace FSharpFar

open FarNet
open Command
open FarStdWriter
open Session
open System
open System.IO

[<System.Runtime.InteropServices.Guid("2b52615b-ea79-46e4-ac9d-78f33599db62")>]
[<ModuleCommand(Name = "FSharpFar", Prefix = "fs")>]
type FsfModuleCommand() =
    inherit ModuleCommand()
    override x.Invoke(sender, e) =
        use cd = new UsePanelDirectory()

        let useEcho() =
            let screen = new UseUserScreen()
            far.UI.WriteLine((sprintf "fs:%s" e.Command), ConsoleColor.DarkGray)
            screen

        match parseCommand e.Command with
        | Quit ->
            use us = useEcho()

            match tryFindMainSession() with
            | Some s -> s.Close()
            | _ -> far.UI.WriteLine "Not opened."

        | Open args ->
            use us = useEcho()

            let ses = match args.With with | Some path -> Session.Get(path) | _ -> getMainSession()
            let interactive = Interactive.Interactive(ses)
            interactive.Open()

        | Code code ->
            use us = useEcho()

            let ses = getMainSession()
            let r = ses.EvalInteraction(Console.Out, code)
            for w in r.Warnings do
                far.UI.WriteLine(formatFSharpErrorInfo w, ConsoleColor.Yellow)
            if r.Exception <> null then
                writeException r.Exception

        | Exec args ->
            use std = new FarStdWriter()
            let ses = match args.With with | Some path -> Session.Get(path) | _ -> getMainSession()
            use writer = new StringWriter()
            let r = ses.EvalScript(writer, args.File)

            if r.Warnings.Length > 0 || r.Exception <> null then
                use us = useEcho()
                far.UI.Write(writer.ToString())
                for w in r.Warnings do
                    far.UI.WriteLine(formatFSharpErrorInfo w, ConsoleColor.Yellow)
                if r.Exception <> null then
                    writeException r.Exception
