namespace FSharpFar
open FarNet
open System

/// Shows the progress info in the window title and the task bar.
type Progress(title) as this =
    static let mutable head = Option<Progress>.None

    let oldWindowTitle = far.UI.WindowTitle
    let tail = head
    do
        far.UI.SetProgressState TaskbarProgressBarState.Indeterminate
        if not (isNull title) then
            far.UI.WindowTitle <- title
        head <- Some this

    interface IDisposable with
        member __.Dispose() =
            head <- tail
            if not (isNull title) then
                far.UI.WindowTitle <- oldWindowTitle
            if tail.IsNone then
                far.UI.SetProgressState TaskbarProgressBarState.NoProgress

    member __.Done() =
        far.UI.SetProgressState TaskbarProgressBarState.NoProgress
        far.UI.SetProgressFlash()
