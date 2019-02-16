namespace FarNet.FSharp
open FarNet
open FarNet.Forms
open System
open System.Diagnostics
open System.Runtime.CompilerServices

// We do not expose anything for posting steps because they are only needed for
// opening panels. We provide jobs for opening panels. If we expose steps users
// may start abusing them.
// @1: Steps are not in sync with jobs, take care:
// -- use Job.FromContinuations ... PostStep
// -- use econt, not throw, or exn "leaks"

[<Sealed>]
type Job =
    /// Posts the Far job for the function.
    /// f: The function invoked in the main Far thread.
    static member inline private PostJob f =
        far.PostJob (Action f)

    /// Creates a job from the callback, see Async.FromContinuations.
    static member inline FromContinuations f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            Job.PostJob (fun () -> f (cont, econt, ccont))
        )

    /// Posts an error dialog for the exception.
    /// The exception Name is shown in the title instead of FullName,
    /// in order to be slightly different from other FarNet errors.
    /// The FullName is still available by [More].
    static member PostShowError exn =
        Job.PostJob (fun () -> far.ShowError (exn.GetType().Name, exn))

    static member private CatchShowError job = async {
        let! res = Async.Catch job
        match res with
        | Choice1Of2 _ -> ()
        | Choice2Of2 exn -> Job.PostShowError exn
    }

    /// Starts the job by Async.Start with exceptions caught and shown as dialogs.
    static member Start job =
        Async.Start <| Job.CatchShowError job

    /// Starts the job by Async.StartImmediate with exceptions caught and shown as dialogs.
    static member StartImmediate job =
        Async.StartImmediate <| Job.CatchShowError job

    // Macros, jobs, and steps are invoked by Far and FarNet in separate queues.
    // As a result, a macro followed by a job or step with `cont` is not good.
    // Thus, we use a pure macro approach: another macro sets the global flag
    // and a separate thread waits for this flag in order to call `cont`.
    static member private envMacroFlag = "FarNet.Async.macro"
    static member private macroSetFlag = sprintf "mf.env('%s', 1, '1')" Job.envMacroFlag

    /// Creates a job from the function dealing with Far.
    /// f: The function with any result.
    static member From f =
        Job.FromContinuations (fun (cont, econt, _) ->
            try
                cont (f ())
            with exn ->
                econt exn
        )

    /// Job helper: Job.StartFrom f ~  Job.Start (Job.From f)
    static member StartFrom f =
        Job.Start (Job.From f)

    /// Job helper: Job.StartImmediateFrom f ~  Job.StartImmediate (Job.From f)
    static member StartImmediateFrom f =
        Job.StartImmediate (Job.From f)

    /// Creates a job from the macro text.
    static member Macro text =
        Async.FromContinuations (fun (cont, econt, _) ->
            // drop the flag
            Environment.SetEnvironmentVariable (Job.envMacroFlag, "0")
            try
                // for clear syntax errors use two macros, do not join
                // original macro
                far.PostMacro text
                // setting the flag
                far.PostMacro Job.macroSetFlag
                // waiting for the flag
                async {
                    while Environment.GetEnvironmentVariable Job.envMacroFlag <> "1" do
                        do! Async.Sleep 50
                    cont ()
                }
                |> Async.Start
            with exn ->
                econt exn
        )

    /// Creates a job from the macro keys.
    static member Keys keys =
        Job.Macro (sprintf "Keys[[%s]]" keys)

    /// The job to cancel the flow.
    static member Cancel () =
        Job.FromContinuations (fun (_, _, ccont) ->
            ccont (OperationCanceledException ())
        )

    /// Waits for the predicate returning true.
    /// delay: Milliseconds to sleep before the first check.
    /// sleep: Milliseconds to sleep after the predicate returning false.
    /// timeout: Maximum waiting time in milliseconds, non positive ~ infinite.
    static member Wait (delay, sleep, timeout, predicate) = async {
        let timeout = if timeout > 0 then timeout else Int32.MaxValue
        let jobPredicate = Job.From predicate

        if delay > 0 then
            do! Async.Sleep delay

        let mutable ok = false
        let sw = Stopwatch.StartNew ()
        while not ok && int sw.ElapsedMilliseconds < timeout do
            let! r = jobPredicate
            ok <- r
            if not ok && sleep > 0 then
                do! Async.Sleep sleep

        return ok
    }

    /// Waits for a few seconds for the predicate returning true and fails if it always gets false.
    static member Wait (predicate, [<CallerFilePath>]?path, [<CallerLineNumber>]?line) = async {
        let! ok = Job.Wait (200, 200, 5000, predicate)
        if not ok then
            Assert.Fail ("Timeout", ?path=path, ?line=line)
    }

    /// Opens the editor and waits for its closing.
    static member FlowEditor (editor: IEditor) = async {
        let mutable closed = false
        do! Job.FromContinuations (fun (cont, econt, ccont) ->
            try
                editor.Closed.Add (fun _ -> closed <- true)
                editor.Open ()
                cont ()
            with exn ->
                econt exn
        )
        //! check for closed in a job
        let! wait = Job.From (fun () ->
            if closed then
                None
            else
                Some (Async.AwaitEvent editor.Closed)
        )
        match wait with
        | Some wait ->
            do! wait |> Async.Ignore
        | None ->
            ()
    }

    /// Opens the viewer and waits for its closing.
    static member FlowViewer (viewer: IViewer) = async {
        let mutable closed = false
        do! Job.FromContinuations (fun (cont, econt, ccont) ->
            try
                viewer.Closed.Add (fun _ -> closed <- true)
                viewer.Open ()
                cont ()
            with exn ->
                econt exn
        )
        //! check for closed in a job
        let! wait = Job.From (fun () ->
            if closed then
                None
            else
                Some (Async.AwaitEvent viewer.Closed)
        )
        match wait with
        | Some wait ->
            do! wait |> Async.Ignore
        | None ->
            ()
    }

    /// Waits until the modal state is over.
    static member SkipModal () = async {
        do! Job.Wait (0, 1000, 0, fun () -> not far.Window.IsModal) |> Async.Ignore
    }

    /// Opens the non modal dialog with the closing function.
    /// dialog: Dialog to open.
    /// closing: Function like Closing handler but with a result, normally an option:
    ///     if isNull args.Control then // canceled
    ///         None
    ///     elif ... then // do not close
    ///         args.Ignore <- true
    ///         None
    ///     else // some result
    ///         Some ...
    static member FlowDialog (dialog: IDialog, closing) =
        Job.FromContinuations (fun (cont, econt, _) ->
            dialog.Closing.Add (fun args ->
                try
                    let r = closing args
                    if not args.Ignore then
                        cont r
                with exn ->
                    if not args.Ignore then
                        econt exn
                    else
                        reraise ()
            )
            dialog.Open ()
        )

    /// Sets panels current or fails.
    static member private EnsurePanels () =
        if far.Window.Kind <> WindowKind.Panels then
            try
                far.Window.SetCurrentAt -1
            with exn ->
                raise (InvalidOperationException ("Cannot switch to panels.", exn))

    /// Opens the panel and waits for its closing.
    static member FlowPanel (panel: Panel) = async {
        do! Job.OpenPanel panel
        do! Job.WaitPanelClosed panel
    }

    /// Opens the specified panel.
    static member OpenPanel (panel: Panel) = async {
        //! in a separate job
        do! Job.From Job.EnsurePanels
        // open
        do! Async.FromContinuations (fun (cont, econt, _) ->
            far.PostStep (fun () ->
                try
                    panel.Open ()
                    cont ()
                with exn ->
                    econt exn
            )
        )
        // check (use PostStep and econt, @1)
        do! Async.FromContinuations (fun (cont, econt, _) ->
            far.PostStep (fun () ->
                try
                    if far.Panel <> upcast panel then
                        invalidOp "OpenPanel did not open the panel."
                    else
                        cont ()
                with exn ->
                    econt exn
            )
        )
    }

    /// Calls the function which opens a panel and returns this panel.
    static member OpenPanel (f) = async {
        //! in a separate job
        do! Job.From Job.EnsurePanels
        // open
        let mutable oldPanel = null
        do! Async.FromContinuations (fun (cont, econt, _) ->
            far.PostStep (fun () ->
                oldPanel <- far.Panel
                try
                    f ()
                    cont ()
                with exn ->
                    econt exn
            )
        )
        // check (use PostStep and econt, @1) and return the new panel
        return! Async.FromContinuations (fun (cont, econt, _) ->
            far.PostStep (fun () ->
                let newPanel = far.Panel
                try
                    match newPanel with
                    | :? Panel as panel when newPanel <> oldPanel ->
                        cont panel
                    | _ ->
                        invalidOp "OpenPanel did not open a module panel."
                with exn ->
                    econt exn
            )
        )
    }

    /// Waits for the specified panel to be closed.
    /// Does nothing if the panel is not opened (already closed).
    static member WaitPanelClosed (panel: Panel) = async {
        let! wait = Job.From (fun () ->
            if far.Panel <> upcast panel && far.Panel2 <> upcast panel then
                None
            else
                Some (Async.AwaitEvent panel.Closed)
        )
        match wait with
        | Some wait ->
            do! wait |> Async.Ignore
        | None ->
            ()
    }

    /// Waits for the panel to be closed with the specified closing handler.
    /// Returns the default if the panel is not opened (already closed).
    /// Other return values are provided by the closing.
    static member WaitPanelClosing (panel: Panel, closing: _ -> 'result) = async {
        let mutable res = Unchecked.defaultof<'result>
        panel.Closing.Add (fun args ->
            try
                let r = closing args
                if not args.Ignore then
                    res <- r
            with exn ->
                far.ShowError ("Closing function error", exn)
        )
        do! Job.WaitPanelClosed panel
        return res
    }
