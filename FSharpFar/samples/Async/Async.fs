
(*
    Prototype helpers for Far async flows.
*)

module FarNet.Async
open FarNet
open System

[<RequireQualifiedAccess>]
module Job =
    /// Posts Far job.
    let post f =
        far.PostJob (Action f)

    /// Posts Far step.
    let postStep f =
        far.PostStep (Action f)

    /// Posts Far macro.
    let postMacro macro =
        post (fun () -> far.PostMacro macro)

    /// Async.FromContinuations as Far job.
    let fromContinuations f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            post (fun () -> f (cont, econt, ccont))
        )

    /// Far job from a function to be posted as job.
    /// f: Far function with any result.
    let fromFunc f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            post (fun () -> cont (f ()))
        )

    /// Far job from a function to be posted as step.
    /// A function normally opens a panel.
    /// f: Far function with any result.
    let fromStep f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            postStep (fun () -> cont (f ()))
        )

    /// Cancels the flow.
    let cancel : Async<unit> =
        fromContinuations (fun (cont, econt, ccont) ->
            ccont (OperationCanceledException ())
        )

    /// Waits for the predicate is true.
    /// delay: Time to sleep before checks.
    /// sleep: Time to sleep if the predicate is false.
    /// timeout: Maximum waiting time, non positive ~ infinite.
    let await delay sleep timeout predicate = async {
        let timeout = if timeout > 0 then timeout else Int32.MaxValue
        let jobPredicate = fromFunc predicate

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

    /// Opens the editor and waits for closing.
    let flowEditor (editor: IEditor) = async {
        let mutable closed = false
        let mutable error = null
        post (fun () ->
            try
                editor.Closed.Add (fun _ -> closed <- true)
                editor.Open ()
            with exn ->
                error <- exn
        )
        let! wait = fromFunc (fun () ->
            if error <> null then
                raise error
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

    /// Opens the panel and waits for closing.
    let flowPanel (panel: Panel) = async {
        let! _ = await 0 1000 0 (fun () ->
            not far.Window.IsModal
        )
        do! fromFunc (fun () ->
            if far.Window.Kind <> WindowKind.Panels then
                try
                    far.Window.SetCurrentAt -1
                with exn ->
                    raise (InvalidOperationException ("Cannot set panels.", exn))
        )
        let mutable closed = false
        do! fromStep (fun () ->
            panel.Closed.Add (fun _ -> closed <- true)
            panel.Open ()
        )
        let! wait = fromFunc (fun () ->
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

    /// Message(text, title) as Far job.
    let message2 text title =
        fromFunc (fun () -> far.Message (text, title))

    /// Message(text, title, options) as Far job.
    let message3 text title options =
        fromFunc (fun () -> far.Message (text, title, options))

    /// Message(text, title, options, buttons) as Far job.
    let message4 text title options buttons =
        fromFunc (fun () -> far.Message (text, title, options, buttons))

/// Posts an exception dialog.
let private postExn exn =
    Job.post (fun () -> far.ShowError (exn.GetType().Name, exn))

module Async =
    /// Starts Far flow with posted exception dialogs.
    let farStart flow =
        Async.StartWithContinuations (flow, ignore, postExn, ignore)
