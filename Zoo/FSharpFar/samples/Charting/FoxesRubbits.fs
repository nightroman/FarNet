
// http://fsharpnews.blogspot.com/2010/07/f-vs-mathematica-parametric-plots.html

module FoxesRubbits
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting

let trajectory g k t =
    let evolve (r, f) =
        let dtrf = 0.0001 * r * f
        r + (1.0 - r / k) * r * g - dtrf, dtrf + (1.0 - g) * f
    Seq.scan (fun s _ -> evolve s) (50.0, 10.0) { 1..t }

let series = new Series(ChartType = SeriesChartType.Line)

for x, y in trajectory 0.02 5e2 1500 do
    series.Points.AddXY(x, y) |> ignore

let area = new ChartArea()

area.AxisX.Title <- "Rabbits"
area.AxisY.Title <- "Foxes"
area.AxisX.Minimum <- 0.0

let chart = new Chart(Dock = DockStyle.Fill)

chart.ChartAreas.Add area
chart.Series.Add series

let form = new Form()
form.Controls.Add chart
form.Load.Add(fun _ -> form.Activate())
