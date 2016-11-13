
## How to use Windows forms and charting

Do not call the Windows forms method `Show()`, it is not working in FSharpFar
without Windows forms event loop. Call the modal `ShowDialog()` instead, in a
separate thread, if the dialog should not block Far Manager.

The provided examples use `System.Windows.Forms.DataVisualization.Charting` and
Windows forms directly without any extra packages. Run `*.fsx` scripts in order
to show sample charts.

For easier charting in F# consider using [FSharp.Charting](https://github.com/fslaborg/FSharp.Charting).
Similarly, in FSharpFar use the modal method `Chart.Show(chart)`, not modeless `chart.ShowChart()`.
This way works quite well. Some possible improvements were suggested, see [#122](https://github.com/fslaborg/FSharp.Charting/issues/122).
