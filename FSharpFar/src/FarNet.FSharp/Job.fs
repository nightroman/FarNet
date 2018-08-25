namespace FarNet.FSharp
open FarNet
open FarNet.Forms
open System

// We do not expose anything for steps because they are only needed for opening
// panels and we provide such jobs. Also, steps are not easy to sync with jobs.
// We may expose proper steps but users may start to use them for no reason.

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

    /// Starts the job with exceptions caught and posted as exception dialogs. It
    /// must be used for production flows (start and use interactively) and may be
    /// used is test flows (start concurrent, test states, drive through jobs).
    static member Start job =
        Async.StartWithContinuations (job, ignore, Job.PostShowError, ignore)

    // Macros, jobs, and steps are invoked by Far and FarNet in separate queues.
    // As a result, a macro followed by a job or step with `cont` is not good.
    // Thus, we use a pure macro approach: another macro sets the global flag
    // and a separate thread waits for this flag in order to call `cont`.
    static member private envMacroFlag = "FarNet.Async.macro"
    static member private macroSetFlag = sprintf "mf.env('%s', 1, '1')" Job.envMacroFlag

    /// Wraps the function as a job.
    /// f: The function with any result.
    static member As f =
        Job.FromContinuations (fun (cont, econt, ccont) ->
            try
                cont (f ())
            with exn ->
                econt exn
        )

    /// Creates a job from the macro text.
    static member Macro text =
        Async.FromContinuations (fun (cont, econt, ccont) ->
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
        Job.FromContinuations (fun (cont, econt, ccont) ->
            ccont (OperationCanceledException ())
        )

    /// Waits for the predicate is true.
    /// delay: Time to sleep before the first check.
    /// sleep: Time to sleep after the predicate is false.
    /// timeout: Maximum waiting time, non positive ~ infinite.
    static member Wait (delay, sleep, timeout, predicate) = async {
        let timeout = if timeout > 0 then timeout else Int32.MaxValue
        let jobPredicate = Job.As predicate

        if delay > 0 then
            do! Async.Sleep delay

        let mutable ok = false
        let mutable elapsed = 0
        while not ok && elapsed < timeout do
            let! r = jobPredicate
            ok <- r
            if not ok then
                do! Async.Sleep sleep
                elapsed <- elapsed + sleep

        return ok
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
        let! wait = Job.As (fun () ->
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
        let! wait = Job.As (fun () ->
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

    /// Sets panels current or fails.
    static member private EnsurePanels () =
        if far.Window.Kind <> WindowKind.Panels then
            try
                far.Window.SetCurrentAt -1
            with exn ->
                raise (InvalidOperationException ("Cannot switch to panels.", exn))

    /// Opens the panel.
    static member OpenPanel (panel: Panel) = async {
        //! in a separate job
        do! Job.As Job.EnsurePanels
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
        // check
        do! Async.FromContinuations (fun (cont, _, _) ->
            far.PostStep (fun () ->
                if far.Panel <> (upcast panel) then
                    invalidOp "Panel is not opened."
                cont ()
            )
        )
    }

    /// Opens the panel and waits for its closing.
    static member FlowPanel (panel: Panel) = async {
        do! Job.OpenPanel panel
        do! Async.AwaitEvent panel.Closed |> Async.Ignore
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
    static member FlowDialog (dialog: IDialog) closing =
        Job.FromContinuations (fun (cont, econt, ccont) ->
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
