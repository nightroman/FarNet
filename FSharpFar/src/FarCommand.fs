
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

namespace FSharpFar

open FarNet
open Session
open System
open System.IO
open Command
open Options
open FarStdWriter
open FarInteractive

[<System.Runtime.InteropServices.Guid "2b52615b-ea79-46e4-ac9d-78f33599db62">]
[<ModuleCommand (Name = "FSharpFar", Prefix = "fs")>]
type FarCommand () =
    inherit ModuleCommand ()
    override x.Invoke (sender, e) =
        let echo () =
            far.UI.WriteLine ((sprintf "fs:%s" e.Command), ConsoleColor.DarkGray)

        let writeResult r =
            for w in r.Warnings do
                far.UI.WriteLine (strErrorFull w, ConsoleColor.Yellow)
            if r.Exception <> null then
                writeException r.Exception

        match parseCommand e.Command with
        | Quit ->
            match tryFindMainSession () with
            | Some s -> s.Close ()
            | _ -> far.UI.WriteLine "Not opened."

        | Open args ->
            let ses = match args.With with | Some path -> Session.FindOrCreate path | _ -> getMainSession ()
            FarInteractive(ses).Open ()

        | Code code ->
            echo ()
            use std = new FarStdWriter ()
            let ses = getMainSession ()
            use writer = new StringWriter ()
            let r = ses.EvalInteraction (writer, code)

            far.UI.Write (writer.ToString ())
            writeResult r

        | Exec args ->
            use std = new FarStdWriter ()
            let ses = Session.FindOrCreate (defaultArg args.With (getConfigPathForFile args.File))
            use writer = new StringWriter ()

            let echo =
                (lazy (echo ())).Force
            
            let issues r =
                if r.Warnings.Length > 0 || r.Exception <> null then
                    echo ()
                    far.UI.Write (writer.ToString ())
                    writeResult r
                    true
                else
                    false
            
            // session errors first or issues may look cryptic
            if ses.Errors.Length > 0 then
                echo ()
                far.UI.Write ses.Errors
            
            // eval anyway, session errors may be warnings
            let r = ses.EvalScript (writer, args.File)
            if issues r then () else

            match args.Code with
            | Some code ->
                let r = ses.EvalInteraction (writer, code)
                issues r |> ignore
            | _ ->
                ()
