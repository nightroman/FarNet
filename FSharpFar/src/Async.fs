module FarNet.Async
open FarNet
open FarNet.Forms
open System

/// Posts the Far job (not for opening panels).
let inline private postJob f =
    far.PostJob (Action f)

/// Posts the Far step (used for opening panels).
let inline private postStep f =
    far.PostStep (Action f)

/// Creates a job from the callback, see Async.FromContinuations.
let inline private jobFromContinuations f =
    Async.FromContinuations (fun (cont, econt, ccont) ->
        postJob (fun () -> f (cont, econt, ccont))
    )

/// Creates a step from the callback, see Async.FromContinuations.
let inline private stepFromContinuations f =
    Async.FromContinuations (fun (cont, econt, ccont) ->
        postStep (fun () -> f (cont, econt, ccont))
    )

// Macros, jobs, and steps are invoked by Far and FarNet in separate queues.
// As a result, a macro followed by a job or step with `cont` is not good.
// Thus, we use a pure macro approach: another macro sets the global flag
// and a separate thread waits for this flag in order to call `cont`.
let private envMacroFlag = "FarNet.Async.macro"
let private macroSetFlag = sprintf "mf.env('%s', 1, '1')" envMacroFlag

[<RequireQualifiedAccess>]
module Job =
    /// Creates a job from the function.
    /// f: Far function with any result.
    let func f =
        jobFromContinuations (fun (cont, econt, ccont) ->
            try
                cont (f ())
            with exn ->
                econt exn
        )

    /// Creates a step from the function which normally opens a panel.
    /// For other functions job are more effective.
    /// f: Far function with any result.
    let step f =
        stepFromContinuations (fun (cont, econt, ccont) ->
            try
                cont (f ())
            with exn ->
                econt exn
        )

    /// Creates a job from the macro text.
    let macro text =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            // drop the flag
            Environment.SetEnvironmentVariable (envMacroFlag, "0")
            try
                // for clear syntax errors use two macros, do not join
                // original macro
                far.PostMacro text
                // setting the flag
                far.PostMacro macroSetFlag
                // waiting for the flag
                async {
                    while Environment.GetEnvironmentVariable envMacroFlag <> "1" do
                        do! Async.Sleep 50
                    cont ()
                }
                |> Async.Start
            with exn ->
                econt exn
        )

    /// Creates a job from the macro keys.
    let keys keys =
        macro (sprintf "Keys[[%s]]" keys)

    /// Creates a job which cancels the flow.
    let cancel : Async<unit> =
        jobFromContinuations (fun (cont, econt, ccont) ->
            ccont (OperationCanceledException ())
        )

    /// Waits for the predicate is true.
    /// delay: Time to sleep before the first check.
    /// sleep: Time to sleep after the predicate is false.
    /// timeout: Maximum waiting time, non positive ~ infinite.
    let wait delay sleep timeout predicate = async {
        let timeout = if timeout > 0 then timeout else Int32.MaxValue
        let jobPredicate = func predicate

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
    let flowEditor (editor: IEditor) = async {
        let mutable closed = false
        do! jobFromContinuations (fun (cont, econt, ccont) ->
            try
                editor.Closed.Add (fun _ -> closed <- true)
                editor.Open ()
                cont ()
            with exn ->
                econt exn
        )
        //! check for closed in a job
        let! wait = func (fun () ->
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
    let flowViewer (viewer: IViewer) = async {
        let mutable closed = false
        do! jobFromContinuations (fun (cont, econt, ccont) ->
            try
                viewer.Closed.Add (fun _ -> closed <- true)
                viewer.Open ()
                cont ()
            with exn ->
                econt exn
        )
        //! check for closed in a job
        let! wait = func (fun () ->
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

    /// Opens the panel and waits for its closing.
    let flowPanel (panel: Panel) = async {
        // skip the modal state
        do! wait 0 1000 0 (fun () -> not far.Window.IsModal) |> Async.Ignore
        // ensure panels
        do! func (fun () ->
            if far.Window.Kind <> WindowKind.Panels then
                try
                    far.Window.SetCurrentAt -1
                with exn ->
                    raise (InvalidOperationException ("Cannot set panels.", exn))
        )
        // open
        let mutable closed = false
        do! step (fun () ->
            panel.Closed.Add (fun _ -> closed <- true)
            panel.Open ()
        )
        //! checks in a job
        let! wait = func (fun () ->
            if far.Panel <> (panel :> IPanel) then
                invalidOp "Panel is not opened."
            if closed then
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
    let flowDialog (dialog: IDialog) closing =
        jobFromContinuations (fun (cont, econt, ccont) ->
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

/// Posts an error dialog for the exception.
/// The exception Name is shown in the title instead of FullName,
/// in order to be slightly different from other FarNet errors.
/// The FullName is still available by [More].
let postShowError exn =
    postJob (fun () -> far.ShowError (exn.GetType().Name, exn))

/// Starts the job with exceptions caught and posted as exception dialogs.
/// It is designed for production use of flows (start and use interactively)
/// and in testing flows (start concurrent, test states, drive through jobs).
let startJob job =
    Async.StartWithContinuations (job, ignore, postShowError, ignore)
