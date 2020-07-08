/// This demo opens 3 dialogs and updates them in parallel.
/// While they are working you can work too: try Esc, F12,
/// switch to other windows, etc. To run for 10 seconds:
///
/// fs: Async.Start (Parallel.flow 10.)
///
/// Far API cannot be used from parallel threads directly.
/// But F# job flows and Far synchronous jobs can do this.

module TestParallel
open FarNet
open FarNet.FSharp
open System
open System.Diagnostics

/// Non-modal dialog with some periodically updated data.
let dialog x y seconds = async {
    // open the dialog showing some number
    let dialog = far.CreateDialog (x, y, x + 51, y + 2)
    let text = dialog.AddText (1, 1, 50, "0")
    do! Job.From dialog.Open

    // simulate some work, increment the number
    let random = Random ()
    let sw = Stopwatch.StartNew ()
    while sw.Elapsed.TotalSeconds < seconds do
        do! Async.Sleep (random.Next 20)
        do! job { text.Text <- string (int text.Text + 1) }

    // close and return the result text
    return! job {
        let result = text.Text
        dialog.Close ()
        return result
    }
}

/// Demo flow with 3 "parallel dialogs".
let flow seconds = async {
    // start parallel dialogs and get results
    let! results =
        [
            dialog 1 1 seconds
            dialog 1 5 seconds
            dialog 1 9 seconds
        ]
        |> Async.Parallel

    // show results
    do! job { far.Message (String.Join (" ", results), "done") }
}

open System.Text.RegularExpressions

[<Test>]
let test = async {
    Async.Start (flow 1.)
    do! Job.Wait (fun () -> Window.IsDialog () && far.Dialog.[0].Text = "done")

    do! job {
        let text = far.Dialog.[1].Text
        let m = Regex.Match (text, "^\d+ \d+ \d+$")
        Assert.True (m.Success && text <> "0 0 0")
    }

    do! Job.Keys "Esc"
    do! job { Assert.NativePanel () }
}
