namespace FarNet.FSharp

/// The base builder literally wrapping a code block.
/// Derived builders have custom overrides, e.g. Run.
type BlockBuilder () =
    member __.Delay (f) =
        f
    member __.Run (f) =
        f ()
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
        finally if (not (isNull (box disposable))) then disposable.Dispose ()
