module internal FSharpFar.Main

open System
open System.IO
open System.Reflection
open FSharp.Compiler.Interactive.Shell

[<assembly: System.Runtime.InteropServices.ComVisible(false)>]
[<assembly: System.CLSCompliant(true)>]
do ()

[<EntryPoint>]
[<STAThread()>]
[<LoaderOptimization(LoaderOptimization.MultiDomainHost)>]
let main _ =
    // FARHOME may be missing if we start from cmd.
    let home = AppDomain.CurrentDomain.BaseDirectory
    Environment.SetEnvironmentVariable("FARHOME", home)

    let argv = System.Environment.GetCommandLineArgs()
    let exe, args = argv.[0], Array.skip 1 argv
    let ini, args =
        if args.Length > 0 && args.[0].ToLower().EndsWith(".ini") then
            Some (Path.GetFullPath args.[0]), Array.skip 1 args
        else
            Config.tryFindFileInDirectory Environment.CurrentDirectory, args

    let argv = [|
        // this app
        exe

        // references for `fsi`
        sprintf @"-r:%s\fsx.exe" home
        sprintf @"-r:%s\FarNet\Modules\FSharpFar\FSharp.Compiler.Service.dll" home

        // configuration, omit [fsi], maybe later use [fsx] in addition
        match ini with
        | Some ini ->
            let config = Config.readFromFile ini
            yield! config.FscArgs
            yield! config.FscFiles
        | None ->
            ()

        // command line parameters
        yield! args
    |]

    let isShadowCopy x = (x = "/shadowcopyreferences" || x = "--shadowcopyreferences" || x = "/shadowcopyreferences+" || x = "--shadowcopyreferences+")
    if AppDomain.CurrentDomain.IsDefaultAppDomain() && argv |> Array.exists isShadowCopy then
        let setupInformation = AppDomain.CurrentDomain.SetupInformation
        setupInformation.ShadowCopyFiles <- "true"
        let helper = AppDomain.CreateDomain("FSI_Domain", null, setupInformation)
        helper.ExecuteAssemblyByName(Assembly.GetExecutingAssembly().GetName()) |> ignore

    try
        let console = InteractiveConsole.ReadLineConsole()
        let getConsoleReadLine (probeToSeeIfConsoleWorks) =
            let consoleIsOperational =
                if probeToSeeIfConsoleWorks then
                    try
                        Console.KeyAvailable |> ignore
                        Console.CursorLeft <- Console.CursorLeft
                        Console.ForegroundColor <> Console.BackgroundColor
                    with _ ->
                        false
                else
                    true
            if consoleIsOperational then
                Some console.ReadLine
            else
                None

        // get and update the configuration with custom GetOptionalConsoleReadLine
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
        let fsiConfig =
            { new FsiEvaluationSessionHostConfig() with
                member __.FormatProvider = fsiConfig.FormatProvider
                member __.FloatingPointFormat = fsiConfig.FloatingPointFormat
                member __.AddedPrinters = fsiConfig.AddedPrinters
                member __.ShowDeclarationValues = fsiConfig.ShowDeclarationValues
                member __.ShowIEnumerable = fsiConfig.ShowIEnumerable
                member __.ShowProperties = fsiConfig.ShowProperties
                member __.PrintSize = fsiConfig.PrintSize
                member __.PrintDepth = fsiConfig.PrintDepth
                member __.PrintWidth = fsiConfig.PrintWidth
                member __.PrintLength = fsiConfig.PrintLength
                member __.ReportUserCommandLineArgs(args) = fsiConfig.ReportUserCommandLineArgs(args)
                member __.EventLoopRun() = fsiConfig.EventLoopRun()
                member __.EventLoopInvoke(f) = fsiConfig.EventLoopInvoke(f)
                member __.EventLoopScheduleRestart() = fsiConfig.EventLoopScheduleRestart()
                member __.UseFsiAuxLib = fsiConfig.UseFsiAuxLib
                member __.StartServer(fsiServerName) = fsiConfig.StartServer(fsiServerName)
                member __.GetOptionalConsoleReadLine(probe) = getConsoleReadLine(probe)
            }

        // make the session and connect its GetCompletions to console
        let fsiSession = FsiEvaluationSession.Create(fsiConfig, argv, Console.In, Console.Out, Console.Error)
        console.SetCompletionFunction(fsiSession.GetCompletions)

        // start the session
        fsiSession.Run()
        0
    with exn ->
        printfn "Exception in %s:" exe
        printfn "%+A" exn
        1
