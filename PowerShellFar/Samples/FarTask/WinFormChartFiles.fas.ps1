<#
.Synopsis
	GUI chart with interaction and panel updates.

.Description
	This script shows a bar chart of file lengths of the current panel.
	Interaction: click on file bars to set the panel file current.

	Note, the chart dialog is modal but it does not block Far Manager.
	The dialog is running in a separate thread and PowerShell session.

	There are two Far jobs: one gets panel files (including plugins),
	another sets panel files current on clicking chart file bars.
#>

using namespace System.Windows.Forms
using namespace System.Windows.Forms.DataVisualization
Add-Type -AssemblyName System.Windows.Forms.DataVisualization

# Far job 1: get panel files
$files = job {
	$Far.Panel.ShownFiles | Where-Object {$_.Length} | Sort-Object Length | Select-Object -Last 20
}

# make chart
$area = [Charting.ChartArea]@{Name = 'Area1'}
$series = [Charting.Series]@{ChartType = 'Bar'; ChartArea = 'Area1'}
$files | .{process{
	$p = $series.Points.Add($_.Length)
	$p.Label = $_.Name
	$p.ToolTip = $_.Name
}}
$chart = [Charting.Chart]@{Dock = 'Fill'}
$chart.Series.Add($series)
$chart.ChartAreas.Add($area)

# handle mouse clicks
$chart.add_MouseClick({
	$hit = $chart.HitTest($_.X, $_.Y)
	if ($hit.PointIndex -lt 0) {return}

	# Far job 2: set the clicked file current in the panel
	job -Arguments $series.Points[$hit.PointIndex].Label {
		$Far.Panel.GoToName($args[0])
	}
})

# show dialog
$form = [Form]@{
	Text = 'Click bars to change current files'
	StartPosition = 'Manual'
	Location = [Drawing.Point]::new(0, 0)
	Size = [Drawing.Size]::new(600, 600)
}
$form.Controls.Add($chart)
$form.add_Load({$this.Activate()})
$null = $form.ShowDialog()
