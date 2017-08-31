
(*
    Prototype helpers Far async flows.
*)

module FarNet.Async
open FarNet
open System
open System.IO

/// Posts the Far job (not for opening panels).
let inline private post f =
    far.PostJob (Action f)

/// Posts the Far step (used for opening panels).
let inline private postStep f =
    far.PostStep (Action f)

[<RequireQualifiedAccess>]
module Job =
    /// Creates the job from function.
    /// f: Far function with any result.
    let func f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            post (fun _ ->
                try
                    cont (f ())
                with exn ->
                    econt exn
            )
        )

    /// Creates the job from function posted as step.
    /// The function normally opens a panel.
    /// f: Far function with any result.
    let step f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            postStep (fun _ ->
                try
                    cont (f ())
                with exn ->
                    econt exn
            )
        )

    /// Creates the job from callback, see Async.FromContinuations.
    let fromContinuations f =
        Async.FromContinuations (fun (cont, econt, ccont) ->
            post (fun _ -> f (cont, econt, ccont))
        )

    /// Creates the job from macro code.
    /// Syntax errors cause normal exceptions.
    /// Runtime errors cause modal error dialogs.
    let macro macro =
        fromContinuations (fun (cont, econt, ccont) ->
            try
                far.PostMacro macro
                post cont
            with exn ->
                econt exn
        )

    /// Creates the job from macro keys.
    /// It ignores invalid keys without errors.
    let keys keys =
        macro (sprintf "Keys[[%s]]" keys)

    /// Creates the job from modal function.
    /// The flow continues before it returns.
    /// That is why the function result is unit.
    /// f: Far function making a modal call in the end.
    let modal f =
        fromContinuations (fun (cont, econt, ccont) ->
            let mutable error = null
            post (fun _ ->
                match error with
                | null ->
                    cont ()
                | _ ->
                    econt error
            )
            try
                f ()
            with exn ->
                error <- exn
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

    /// Opens the editor and waits for closing.
    let flowEditor (editor: IEditor) = async {
        let mutable closed = false
        let mutable error = null
        post (fun _ ->
            try
                editor.Closed.Add (fun _ -> closed <- true)
                editor.Open ()
            with exn ->
                error <- exn
        )
        let! wait = func (fun _ ->
            match error with
            | null ->
                if closed then
                    None
                else
                    Some (Async.AwaitEvent editor.Closed)
            | _ ->
                raise error
        )
        match wait with
        | Some wait ->
            do! wait |> Async.Ignore
        | None ->
            ()
    }

    /// Opens the viewer and waits for closing.
    let flowViewer (viewer: IViewer) = async {
        let mutable closed = false
        let mutable error = null
        post (fun _ ->
            try
                viewer.Closed.Add (fun _ -> closed <- true)
                viewer.Open ()
            with exn ->
                error <- exn
        )
        let! wait = func (fun _ ->
            match error with
            | null ->
                if closed then
                    None
                else
                    Some (Async.AwaitEvent viewer.Closed)
            | _ ->
                raise error
        )
        match wait with
        | Some wait ->
            do! wait |> Async.Ignore
        | None ->
            ()
    }

    /// Opens the panel and waits for closing.
    let flowPanel (panel: Panel) = async {
        let! _ = await 0 1000 0 (fun _ ->
            not far.Window.IsModal
        )
        do! func (fun _ ->
            if far.Window.Kind <> WindowKind.Panels then
                try
                    far.Window.SetCurrentAt -1
                with exn ->
                    raise (InvalidOperationException ("Cannot set panels.", exn))
        )
        let mutable closed = false
        do! step (fun _ ->
            panel.Closed.Add (fun _ -> closed <- true)
            panel.Open ()
        )
        let! wait = func (fun _ ->
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

/// Posts an exception dialog.
let postExn exn =
    // Use Name instead of FullName in order to distinguish from native FarNet
    // at least for testing this fact. FullName is still available by [More].
    post (fun _ -> far.ShowError (exn.GetType().Name, exn))

module Async =
    /// Starts Far flow with exceptions caught and posted as exception dialogs.
    let farStart flow =
        Async.StartWithContinuations (flow, ignore, postExn, ignore)
