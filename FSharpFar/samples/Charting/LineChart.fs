
module LineChart
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting

type LineChartForm (title, xs : float seq) as self =
    inherit Form (Text = title)
    let chart = new Chart (Dock = DockStyle.Fill)
    let area = new ChartArea (Name = "Area1")
    let series = new Series ()
    do
        series.ChartType <- SeriesChartType.Line
        xs |> Seq.iter (series.Points.Add >> ignore)
        series.ChartArea <- "Area1"
        chart.Series.Add series
        chart.ChartAreas.Add area
        self.Controls.Add chart
        self.Load.Add (fun _ -> self.Activate ())
