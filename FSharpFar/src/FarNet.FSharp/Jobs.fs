namespace FarNet.FSharp
open FarNet
open FarNet.Forms
open System

(*
Examine job error dialogs. Titles must be exception short names.
Full names mean leaked exceptions handled and shown by the core.

Do not use PostStep(), wait for steps instead, when appropriate.
*)

[<AbstractClass; Sealed>]
type Jobs =
    /// Posts the job, waits until it is called, gets its result (FarNet.Tasks.Job).
    static member Job(job: unit -> _) = async {
        return! Tasks.Job(job) |> Async.AwaitTask
    }

    /// Posts the job, waits until it is called and the core gets control (FarNet.Tasks.Run).
    static member Run(job: unit -> unit) = async {
        do! Tasks.Run(Action job) |> Async.AwaitTask
    }

    /// Posts the keys and waits until they are processed (FarNet.Tasks.Keys).
    static member Keys keys = async {
        do! Tasks.Keys keys |> Async.AwaitTask
    }

    /// Posts the macro and waits until it is processed (FarNet.Tasks.Macro).
    static member Macro text = async {
        do! Tasks.Macro text |> Async.AwaitTask
    }

    /// Opens the editor and waits until it is closed (FarNet.Tasks.Editor).
    static member Editor(editor: IEditor) = async {
        do! Tasks.Editor editor |> Async.AwaitTask
    }

    /// Opens the viewer and waits until it is closed (FarNet.Tasks.Viewer).
    static member Viewer(viewer: IViewer) = async {
        do! Tasks.Viewer viewer |> Async.AwaitTask
    }

    /// Opens the dialog and waits until it is closed (FarNet.Tasks.Dialog).
    static member Dialog(dialog: IDialog) = async {
        do! Tasks.Dialog dialog |> Async.AwaitTask
    }

    /// Opens the dialog, waits until it is closed with the closing job and get the job result (FarNet.Tasks.Dialog).
    static member Dialog(dialog: IDialog, closing: ClosingEventArgs -> 't) = async {
        return! Tasks.Dialog(dialog, Func<ClosingEventArgs, 't> closing) |> Async.AwaitTask
    }

    /// Opens the panel and waits until it is opened (FarNet.Tasks.OpenPanel).
    static member OpenPanel(panel: Panel) = async {
        do! Tasks.OpenPanel(panel) |> Async.AwaitTask
    }

    /// Posts the job which opens a panel, waits until the panel is opened, returns the panel (FarNet.Tasks.OpenPanel).
    static member OpenPanel(job: unit -> unit) = async {
        return! Tasks.OpenPanel(job) |> Async.AwaitTask
    }

    /// Waits until the specified panel is closed (FarNet.Tasks.WaitPanelClosed).
    static member WaitPanelClosed(panel: Panel) = async {
        do! Tasks.WaitPanelClosed(panel) |> Async.AwaitTask
    }

    /// Waits until the panel is closed with the specified closing job and gets the closing result (FarNet.Tasks.WaitPanelClosing).
    static member WaitPanelClosing(panel: Panel, closing: PanelEventArgs -> 't) = async {
        return! Tasks.WaitPanelClosing(panel, Func<PanelEventArgs, 't> closing) |> Async.AwaitTask
    }

    /// Opens the panel and waits until it is closed (FarNet.Tasks.Panel).
    static member Panel(panel: Panel) = async {
        do! Tasks.Panel panel |> Async.AwaitTask
    }

    /// Waits until the job returns true and returns true before the timeout (FarNet.Tasks.Wait).
    /// delay: Milliseconds to delay when the job returns false.
    /// timeout: Maximum waiting time in milliseconds, non positive ~ infinite.
    /// job: Returns true to stop waiting.
    static member Wait(delay, timeout, (job: unit -> bool)) = async {
        return! Tasks.Wait(delay, timeout, Func<bool> job) |> Async.AwaitTask
    }

    /// Creates an async job from the callback, see `Async.FromContinuations`.
    static member inline FromContinuations f =
        Async.FromContinuations(fun (cont, econt, ccont) ->
            far.PostJob(fun () -> f (cont, econt, ccont))
        )

    /// Posts a job which shows the exception dialog.
    /// The title is exception `Name` instead of `FullName`,
    /// in order to be slightly different from the core dialog.
    static member PostShowError(exn: exn) =
        // _201221_2o This helps to reveal bugs. When our catch is not called unexpectedly then
        // FarNet error dialog is shown with the full exception name. We show short names here.

        let exn = Works.Kit.UnwrapAggregateException exn
        if exn :? OperationCanceledException then
            // _220730 ignore canceled noise
            // FarNet.ScottPlot / PanelFilesLive.fsx / async {..} |> Jobs.StartImmediate / .. do! plot.ShowAsync(3000) |> Async.AwaitTask
            ()
        else
            //_201221_2o use short name
            far.PostJob(fun () -> far.ShowError(exn.GetType().Name, exn))

    static member private CatchShowError job = async {
        match! Async.Catch job with
        | Choice2Of2 exn -> Jobs.PostShowError exn
        | _ -> ()
    }

    /// Starts the async job by `Async.Start` with exceptions shown as dialogs.
    static member Start job =
        Jobs.CatchShowError job |> Async.Start

    /// Starts the async job by `Async.StartImmediate` with exceptions shown as dialogs.
    static member StartImmediate job =
        Jobs.CatchShowError job |> Async.StartImmediate

    /// Cancels the current async job.
    static member Cancel() =
        Jobs.FromContinuations(fun (_, _, ccont) ->
            ccont (OperationCanceledException())
        )

    /// Waits until the modal state is over.
    static member WaitModeless() = async {
        do! Jobs.Wait(1000, 0, fun () -> not far.Window.IsModal) |> Async.Ignore
    }

[<AutoOpen>]
module JobBuilder =
    /// Makes `job` expressions.
    type JobBuilder() =
        inherit BlockBuilder()
        member __.Run(f) =
            Jobs.Job f

    /// Expression `job {...}` expands to `Jobs.Job(fun () -> ...)`.
    /// Consider using `Jobs.Job` for code with loops, `try`, `use`.
    let job = JobBuilder()

    /// Makes `run` expressions.
    type RunBuilder() =
        inherit BlockBuilder()
        member __.Run(f) =
            Jobs.Run f

    /// Expression `run {...}` expands to `Jobs.Run(fun () -> ...)`.
    /// Consider using `Jobs.Run` for code with loops, `try`, `use`.
    let run = RunBuilder()
