
#r "System.Windows.Forms.DataVisualization.dll"
#load "FoxesRubbits.fs"
open FoxesRubbits

async {
    do! Async.SwitchToNewThread ()
    form.ShowDialog() |> ignore
}
|> Async.StartImmediate
