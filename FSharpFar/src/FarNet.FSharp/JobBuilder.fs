[<AutoOpen>]
module FarNet.FSharp.JobBuilder
open FarNet.FSharp

type JobBuilder () =
    member __.Run (f) =
        Job.From f
    member __.Delay (f) =
        f
    member __.Zero () =
        ()
    member __.Return (x) =
        x
    member __.Combine (a, b) =
        b a
    member __.For (sequence, body) =
        for item in sequence do
            body item
    member __.While (guard, body) =
        while guard () do
            body ()
    member __.TryWith (body, handler) =
        try body ()
        with exn -> handler exn
    member __.TryFinally (body, compensation) =
        try body ()
        finally compensation ()
    member __.Using (disposable:#System.IDisposable, body) =
        try body disposable
        finally if not (isNull disposable) then disposable.Dispose ()

/// Job helper: job {...} ~ Job.From (fun () -> ...)
let job = JobBuilder ()
