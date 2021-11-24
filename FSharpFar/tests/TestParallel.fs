module TestParallel
open Parallel
open FarNet
open FarNet.FSharp
open System.Text.RegularExpressions

[<Test>]
let test = async {
    Async.Start(demo 1.)
    do! Assert.Wait(fun () -> Window.IsDialog() && far.Dialog[0].Text = "done")

    do! job {
        let text = far.Dialog[1].Text
        let m = Regex.Match(text, "^\d+ \d+ \d+$")
        Assert.True(m.Success && text <> "0 0 0")
    }

    do! Jobs.Keys "Esc"
    do! job { Assert.NativePanel() }
}
