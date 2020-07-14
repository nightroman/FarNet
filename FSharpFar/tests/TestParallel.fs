/// This demo opens 3 dialogs and updates them in parallel.
/// While they are working you can work too: try Esc, F12,
/// switch to other windows, etc. To run for 10 seconds:
///
/// fs: Async.Start (Parallel.flow 10.)
///
/// Far API cannot be used from parallel threads directly.
/// But F# job flows and Far synchronous jobs can do this.

module TestParallel
open Parallel
open FarNet
open FarNet.FSharp
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
