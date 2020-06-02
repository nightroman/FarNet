
#r "System.Windows.Forms.DataVisualization.dll"
#load "FoxesRubbits.fs"
open FoxesRubbits

async { form.ShowDialog() |> ignore }
|> if fsi.CommandLineArgs.[0].EndsWith(".fsx") then Async.RunSynchronously else Async.Start
