// Shows box plots of file openings and opening points.
// Run LogIdle.ps1 first for CSV.

open System
open System.Drawing
open Deedle
open FSharp.Charting

type IRow =
    abstract Path : string
    abstract Idle : float

let file = Environment.ExpandEnvironmentVariables(@"%TEMP%\z.VesselIdle.csv")
let rows = Frame.ReadCsv(file, inferTypes=false).GetRowsAs<IRow>().Values

let pathIdles =
    rows
    |> Seq.groupBy (fun row -> row.Path)
    |> Seq.filter (fun (_, rows) -> Seq.length rows > 4)
    |> Seq.map (fun (path, rows) -> (path, rows |> Seq.map (fun row -> row.Idle)))

let pathPoints =
    pathIdles
    |> Seq.mapi (fun i (path, idles) -> (path, idles |> Seq.map (fun idle -> (i + 1, idle))))

fun () ->
    // number labels
    let pathIdles = pathIdles |> Seq.mapi (fun i (_, idles) -> (i + 1, idles))
    seq {
        yield Chart.BoxPlotFromData(pathIdles, Name="Box")
        yield!
            pathPoints
            |> Seq.map (fun (path, points) ->
                Chart.Point(points, Color=Color.Black, Name=path)
            )
    }
    |> Chart.Combine
    |> Chart.WithXAxis(Min=0)
    |> Chart.WithYAxis(MajorGrid=ChartTypes.Grid(Interval=1))

|> Chart.Show
