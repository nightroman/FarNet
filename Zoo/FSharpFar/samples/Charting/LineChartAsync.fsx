#r "System.Windows.Forms.DataVisualization.dll"
#load "LineChart.fs"
open LineChart

let data = [
    for i in 1..1000 do
        yield sin (float i / 100.0)
]

let form = new LineChartForm("sin", data)

async { form.ShowDialog() |> ignore }
|> if fsi.CommandLineArgs[0].EndsWith(".fsx") then Async.RunSynchronously else Async.Start
