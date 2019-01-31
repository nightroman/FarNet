namespace FSharpFar
open FarNet
open Session
open System
open System.IO
open FarStdWriter
open FarInteractive
open Config

[<System.Runtime.InteropServices.Guid "2b52615b-ea79-46e4-ac9d-78f33599db62">]
[<ModuleCommand (Name = "FSharpFar", Prefix = "fs")>]
type FarCommand () =
    inherit ModuleCommand ()
    override __.Invoke (_, e) =
        let echo () =
            far.UI.WriteLine ((sprintf "fs:%s" e.Command), ConsoleColor.DarkGray)

        let writeResult r =
            for w in r.Warnings do
                far.UI.WriteLine (strErrorFull w, ConsoleColor.Yellow)
            if not (isNull r.Exception) then
                writeException r.Exception

        match Command.parseCommand e.Command with
        | Command.Quit ->
            match tryFindMainSession () with
            | Some s -> s.Close ()
            | _ -> far.UI.WriteLine "Not opened."

        | Command.Open args ->
            let ses = match args.With with | Some path -> Session.GetOrCreate path | _ -> getMainSession ()
            FarInteractive(ses).Open ()

        | Command.Code code ->
            echo ()
            use _std = new FarStdWriter ()
            let ses = getMainSession ()
            use writer = new StringWriter ()
            let r = ses.EvalInteraction (writer, code)

            far.UI.Write (writer.ToString ())
            writeResult r

        | Command.Exec args ->
            use _std = new FarStdWriter ()
            let ses =
                match args.With, args.File with
                | Some configPath, _ -> configPath
                | _, Some filePath -> getConfigPathForFile filePath
                | _ -> farMainConfigPath
                |> Session.GetOrCreate

            let echo =
                (lazy (echo ())).Force

            use writer = new StringWriter ()
            let validate r =
                if r.Warnings.Length > 0 || not (isNull r.Exception) then
                    echo ()
                    far.UI.Write (writer.ToString ())
                    writeResult r
                    false
                else
                    true

            // session errors first or issues may look cryptic
            if ses.Errors.Length > 0 then
                echo ()
                far.UI.Write ses.Errors

            // eval anyway, session errors may be warnings
            let ok =
                match args.File with
                | Some file ->
                    let r = ses.EvalScript (writer, file)
                    validate r
                | None ->
                    true

            match ok, args.Code with
            | true, Some code ->
                let r = ses.EvalInteraction (writer, code)
                validate r |> ignore
            | _ ->
                ()

        | Command.Compile args ->
            use _progress = new Progress "Compiling..."

            let path =
                match args.With with
                | Some path ->
                    path
                | None -> 
                    match farTryPanelDirectory () |> Option.bind tryConfigPathInDirectory with
                    | Some path ->
                        path
                    | None ->
                        invalidOp "Cannot find configuration file."
            
            let config = readConfigFromFile path
            
            let errors, code = Checker.compile config |> Async.RunSynchronously
            if errors.Length > 0 then
                use writer = new StringWriter ()
                for error in errors do
                    writer.WriteLine (strErrorLine error)
                showTempText (writer.ToString ()) "Errors"
            ()
