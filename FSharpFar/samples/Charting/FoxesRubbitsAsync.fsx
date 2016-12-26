
#r "System.Windows.Forms.DataVisualization.dll"
#load "FoxesRubbits.fs"
open FoxesRubbits

Async.Start (async { form.ShowDialog() |> ignore })
