<#
.Synopsis
	Opens CSV file or DataFrame for browsing in the panel.
	Author: Roman Kuzmin

.Description
	Requires: https://github.com/nightroman/DataFrame

	Panel columns:
	- Name: Table and columns.
	- Type: Column data types.
	- Rows: Row counts, excluding nulls for columns.
	- Max, Min, Mean: Statistics for numeric columns.

	Keys:
	- [Enter] opens data table or column panels.

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
)

#requires -version 7.4
trap {Write-Error $_}
$ErrorActionPreference = 1
if ($Host.Name -ne 'FarHost') {throw 'Please run with FarNet.PowerShellFar.'}

Import-Module DataFrame

if ($Source -is [string]) {
	$title = [System.IO.Path]::GetFileName($Source)
	if (!$Separator) {
		$Separator = ([System.IO.Path]::GetExtension($Source) -eq '.tsv' ? 't' : 'c')
	}
	$df = Import-DataFrame $Source -GuessCount 10, 1e6, 1e7 -Separator $Separator -RowCount $RowCount -NoHeader:$NoHeader -RenameColumn
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
	}
	else {
		$df = New-DataFrame $data
		$dt = $df.ToTable()
		$dt.AcceptChanges()

		$child = [PowerShellFar.DataPanel]::new()
		$child.Table = $dt
		$child.OpenChild($Panel)
	}
}

$Panel.Open()
