<#
.Synopsis
	Opens CSV file or DataFrame for browsing in the panel.
	Author: Roman Kuzmin

.Description
	Requires: https://github.com/nightroman/DataFrame
	Optional: https://www.nuget.org/packages/FarNet.ScottPlot

	Panel columns:
	- Name: Table and columns.
	- Type: Column data types.
	- Rows: Row counts, excluding nulls for columns.
	- Max, Min, Mean: Statistics for numeric columns.

	Keys:
	- [Enter] opens data table or column panels
	- [F1] panel help menu, plots:
		- Histogram, for numeric columns
		- Top 10 50 .., top used entries
		- Scatter of two selected columns

.Parameter Source
		Specifies CSV file path or DataFrame instance.

.Parameter Separator
		Specifies the file separator.
		Defaults to tab for .tsv, otherwise comma.

.Parameter RowCount
		The maximum number of file rows to read.
		Use -1 for no limit. Defaults to 100,000.

.Parameter NoHeader
		Tells that the file has no header.

.Parameter IndexColumn
		Tells to add the index column.
#>

[CmdletBinding()]
param(
	[Parameter(Mandatory=1)]
	[object]$Source
	,
	[char]$Separator
	,
	[int]$RowCount = 100000
	,
	[switch]$NoHeader
	,
	[switch]$IndexColumn
)

#requires -Version 7.4 -Modules DataFrame
trap {Write-Error $_}
$ErrorActionPreference = 1
if ($Host.Name -ne 'FarHost') {throw 'Please run with FarNet.PowerShellFar.'}

if ($Source -is [string]) {
	$title = [System.IO.Path]::GetFileName($Source)
	if (!$Separator) {
		$Separator = ([System.IO.Path]::GetExtension($Source) -eq '.tsv' ? 't' : 'c')
	}
	$df = Import-DataFrame $Source -GuessCount 10, 1e6, 1e7 -Separator $Separator -RowCount $RowCount -NoHeader:$NoHeader -IndexColumn:$IndexColumn -RenameColumn
}
else {
	$title = 'DataFrame'
	$df = $Source -as [Microsoft.Data.Analysis.DataFrame]
	if (!$df) {
		throw 'Source must be String or DataFrame.'
	}
}

### Explorer
$Explorer = [PowerShellFar.PowerExplorer]::new('d196bd2d-47c4-421f-a3f0-5d87c534f87f')
$Explorer.Data = @{
	DataFrame = $df
}

### AsGetFiles
$Explorer.AsGetFiles = {
	param($Explorer)
	$df = $Explorer.Data.DataFrame

	[FarNet.SetFile]@{
		Data = $df
		Name = ''
		Description = 'Table'
		Columns = @($df.Rows.Count)
	}

	foreach($column in $df.Columns) {
		if ($column.Name -eq 'IndexColumn') {
			continue
		}
		$isNumericColumn = $column.IsNumericColumn()
		[FarNet.SetFile]@{
			Data = $column
			Name = $column.Name
			Description = $column.DataType.Name
			Columns = @(
				$column.Length - $column.NullCount
				$isNumericColumn ? $column.Max() : $null
				$isNumericColumn ? $column.Min() : $null
				$isNumericColumn ? [float]$column.Mean() : $null
			)
		}
	}
}

### Panel
$Panel = [PowerShellFar.AnyPanel]::new($Explorer)
$Panel.Title = $title
$Panel.ViewMode = 0
$Panel.DotsMode = 'Off'
$Panel.SortMode = 'Unsorted'

### Plan
$plan = [FarNet.PanelPlan]::new()
$cName = [FarNet.SetColumn]@{ Kind = 'N'; Name = 'Name' }
$cType = [FarNet.SetColumn]@{ Kind = "Z"; Name = "Type"; Width = 9 }
$cRows = [FarNet.SetColumn]@{ Kind = 'C0'; Name = 'Rows' }
$cMax = [FarNet.SetColumn]@{ Kind = 'C1'; Name = 'Max' }
$cMin = [FarNet.SetColumn]@{ Kind = 'C2'; Name = 'Min' }
$cMean = [FarNet.SetColumn]@{ Kind = 'C3'; Name = 'Mean' }
$plan.Columns = $cName, $cType, $cRows, $cMax, $cMin, $cMean
$plan.StatusColumns = $plan.Columns
$Panel.SetPlan(0, $plan)

### AsOpenFile
$Panel.AsOpenFile = {
	param($Panel, $_)
	$data = $_.File.Data

	if ($data -is [Microsoft.Data.Analysis.DataFrame]) {
		$dt = $data.ToTable()
		$dt.AcceptChanges()

		$child = [PowerShellFar.DataPanel]::new()
		$child.Table = $dt
		$child.OpenChild($Panel)
		return
	}

	if ($data -is [Microsoft.Data.Analysis.DataFrameColumn]) {
		$df = New-DataFrame $data
		$dt = $df.ToTable()
		$dt.AcceptChanges()

		$child = [PowerShellFar.DataPanel]::new()
		$child.Table = $dt
		$child.OpenChild($Panel)
		return
	}
}

### Help menu
$Panel.add_MenuCreating({
	$file = $_.SelectedFiles
	if ($file.Count -eq 2) {
		$c1 = $file[0].Data -as [Microsoft.Data.Analysis.DataFrameColumn]
		$c2 = $file[1].Data -as [Microsoft.Data.Analysis.DataFrameColumn]
		if ($c1 -and $c2) {
			if (!$c1.IsNumericColumn()) {$c1 = DFConvertColumn $c1}
			if (!$c2.IsNumericColumn()) {$c2 = DFConvertColumn $c2}
			$_.Menu.Add("$($c1.Name) -> $($c2.Name)", {DFShowColumnScatter $_.Item.Data[0] $_.Item.Data[1]}).Data = $c1, $c2
			$_.Menu.Add("$($c2.Name) -> $($c1.Name)", {DFShowColumnScatter $_.Item.Data[0] $_.Item.Data[1]}).Data = $c2, $c1
		}
		return
	}

	$file = $_.CurrentFile
	if (!$file) {
		return
	}

	$data = $file.Data -as [Microsoft.Data.Analysis.DataFrameColumn]
	if (!$data -or $data.Length - $data.NullCount -le 0) {
		return
	}

	if ($data.IsNumericColumn()) {
		$_.Menu.Add('Histogram', {DFShowColumnHistogram $_.Item.Data}).Data = $data
	}

	$_.Menu.Add('Top 10', {DFShowColumnCounts $_.Item.Data 10}).Data = $data
	$_.Menu.Add('Top 50', {DFShowColumnCounts $_.Item.Data 50}).Data = $data
	$_.Menu.Add('Top ..', {DFShowColumnCounts $_.Item.Data 0}).Data = $data
})

function global:DFConvertColumn {
	param(
		[Parameter(Mandatory=1)]
		[Microsoft.Data.Analysis.DataFrameColumn]$Column
	)

	$r = New-Int32Column $Column.Name -Length $Column.Length
	$map = [hashtable]::new([System.StringComparer]::Ordinal)
	for($$ = 0; $$ -lt $Column.Length; ++$$) {
		$key = $Column[$$]
		$index = $map[$key]
		if ($null -eq $index) {
			$map[$key] = $index = $$
		}
		$r[$$] = $index
	}
	,$r
}

function global:DFAssertScottPlot {
	try {
		Add-Type -Path @(
			"$env:FARHOME\FarNet\Lib\FarNet.ScottPlot\FarNet.ScottPlot.dll"
			"$env:FARHOME\FarNet\Lib\FarNet.ScottPlot\ScottPlot.dll"
		)
	}
	catch {
		throw "Cannot load FarNet.ScottPlot, is it installed?"
	}
}

function global:DFShowColumnScatter {
	param(
		[Parameter(Mandatory=1)]
		[Microsoft.Data.Analysis.DataFrameColumn]$Column1
		,
		[Parameter(Mandatory=1)]
		[Microsoft.Data.Analysis.DataFrameColumn]$Column2
	)

	DFAssertScottPlot

	$xs = [double[]]$Column1
	$ys = [double[]]$Column2

	$plot = [FarNet.ScottPlot.FormPlot]::new("$($Column1.Name) -> $($Column2.Name)")
	$set1 = $plot.AddScatter($xs, $ys, $null, 0)
	$null = $plot.XAxis.Label($Column1.Name)
	$null = $plot.YAxis.Label($Column2.Name)
	$plot.Show()
}

function global:DFShowColumnHistogram {
	param(
		[Parameter(Mandatory=1)]
		[Microsoft.Data.Analysis.DataFrameColumn]$Column
	)

	DFAssertScottPlot

	$N = 50
	$min = $Column.Min()
	$max = $Column.Max()
	$values = [double[]]$Column

	$hist = [ScottPlot.Statistics.Histogram]::new($min, $max, $N)
	$hist.AddRange($values)

	$plot = [FarNet.ScottPlot.FormPlot]::new($Column.Name)
	$set1 = $plot.AddBar($hist.Counts, $hist.Bins)
	$set1.BarWidth = ($max - $min) / $N
	$set2 = $plot.AddFunction($hist.GetProbabilityCurve($values), 'Green', 2, 'Dash')
	$set2.YAxisIndex = 1
	$null = $plot.YAxis.Label('Count')
	$null = $plot.YAxis2.Label('Probability')
	$plot.YAxis2.Ticks($true)
	$plot.SetAxisLimits($null, $null, 0.0, $null, 0, 0)
	$plot.SetAxisLimits($null, $null, 0.0, 1.1, 0, 1)
	$plot.Show()
}

function global:DFShowColumnCounts {
	param(
		[Parameter(Mandatory=1)]
		[Microsoft.Data.Analysis.DataFrameColumn]$Column
		,
		[int]$N
	)

	DFAssertScottPlot

	$df = $Column.ValueCounts().OrderByDescending('Counts')

	if ($N -le 0) {
		try {$N = $Far.Input('Count', $null, 'Top', $df.Rows.Count)}
		catch {}
		if ($N -le 0) {
			return
		}
	}

	if ($N -lt $df.Rows.Count) {
		$df = $df.Head($N)
	}

	[double[]]$values = $df['Counts']
	[string[]]$labels = $df['Values'].ForEach{"$_"}.ForEach{$_.Substring(0, [Math]::Min($_.Length, 25))}

	$plot = [FarNet.ScottPlot.FormPlot]::new($Column.Name)
	$set1 = $plot.AddBar($values, $null)
	$plot.XTicks($labels)
	$plot.XAxis.TickLabelStyle($null, $null, $null, $null, 60)
	$plot.YLabel('Count')
	$plot.Show()
}

$Panel.Open()
