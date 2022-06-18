// Shows histogram of idle times.
// Run LogIdle.ps1 first for CSV.

open System
open Deedle
open FSharp.Charting

let file = Environment.ExpandEnvironmentVariables(@"%TEMP%\z.VesselIdle.csv")
let data = Frame.ReadCsv(file, schema="Path (string), Idle (float)")

fun () ->
    Chart.Histogram(Series.values data?Idle, Intervals=12)
    |> Chart.WithXAxis(Min=0.0)

|> Chart.Show
