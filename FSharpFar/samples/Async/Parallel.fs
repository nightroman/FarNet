
/// Far API cannot be used from parallel threads directly.
/// But F# job flows and Far synchronous jobs can do this.
///
/// This demo opens 3 dialogs and updates them in parallel.
/// While they are working you can work too: try Esc, F12,
/// switch to other windows, etc. To run for 10 seconds:
///
/// fs: //exec file=Parallel.fs ;; FarNet.FSharp.Job.Start (Parallel.flow 10.)

module Parallel
open FarNet
open FarNet.FSharp
open System
open System.Diagnostics

let dialog x y seconds = async {
    // open non modal dialog showing some number
    let dialog = far.CreateDialog (x, y, x + 51, y + 2)
    let text = dialog.AddText (1, 1, 50, "0")
    do! Job.As dialog.Open

    // simulate some work, increment the number
    let random = Random ()
    let sw = Stopwatch.StartNew ()
    while sw.Elapsed.TotalSeconds < seconds do
        do! Async.Sleep (random.Next 20)
        do! Job.As (fun () -> text.Text <- string (int text.Text + 1))

    // close and return result
    return! Job.As (fun () ->
        let result = text.Text
        dialog.Close ()
        result
    )
}

/// Demo flow.
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
    do! Job.As (fun () ->
        far.Message (String.Join (" ", results), "done")
    )
}

open Test
open System.Text.RegularExpressions

/// Auto test.
let test = async {
    Async.Start (flow 1.)
    do! wait (fun () -> isDialog () && dt 0 = "done")

    do! Job.As (fun () ->
        let text = dt 1
        let m = Regex.Match (text, "^\d+ \d+ \d+$")
        if text = "0 0 0" || not m.Success then failwith "unexpected result"
    )

    do! Job.Keys "Esc"
    do! test isFarPanel
}
