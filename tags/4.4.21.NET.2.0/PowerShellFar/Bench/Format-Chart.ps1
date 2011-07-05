
<#
.SYNOPSIS
	Formats output as a table with the last chart column.
	Author: Roman Kuzmin

.DESCRIPTION
	The chart column is based on the last property, numeric, indeed.

.EXAMPLE
	# Process working sets; bar characters make shadows effect
	Get-Process | Format-Chart Name, WS -Bar ([char]9600) -Space ([char]9617)

	# Role of the parameter Min
	Get-Process | ?{$_.WS -gt 10Mb} | Format-Chart Name, WS
	Get-Process | ?{$_.WS -gt 10Mb} | Format-Chart Name, WS -Min 0
#>

param
(
	[object[]]
	# Properties where the last one is numeric for a chart.
	$Property = $(throw "Please, set -Property."),

	# Min axis value. Default: the minimum value.
	$Min,

	# Max axis value. Default: the mamimum value.
	$Max,

	[int]
	# Chart column width. Default: 1/2 of screen buffer.
	$Width = ($Host.UI.RawUI.BufferSize.Width/2),

	[string]
	# Character to fill chart bars.
	$BarChar = [char]9632,

	[string]
	# Character to fill chart space.
	$SpaceChar = ' ',

	[object[]]
	# Input objects. Default: objects from pipeline.
	$InputObject,

	[switch]
	# Use logarithmic scale.
	$Logarithmic
)
if ($args) { throw "Unknown parameters: $args" }
Set-StrictMode -Version 2

# select properties together with an extra chart column
$data = $(if ($InputObject) { $InputObject } else { @($Input) }) | Select-Object ($Property + 'Chart')
$name = $Property[-1]

# get min and max, set range
$mm = $data | Measure-Object $name -Minimum -Maximum
if ($Min -eq $null) { $Min = $mm.Minimum }
if ($Max -eq $null) { $Max = $mm.Maximum }
$range = $Max - $Min
if ($range -lt 0) { throw "Invalid Min, Max: $Min, $Max" }
if ($range -eq 0) { $range = 1 }

# fill the chart column
foreach($_ in $data) {
	if ($Logarithmic) {
		$coeff = [math]::Log(($_.$name - $Min + 1), ($range + 1))
	}
	else {
		$coeff = ($_.$name - $Min) / $range
	}
	if ($coeff -lt 0) { $coeff = 0 }
	elseif ($coeff -gt 1) { $coeff = 1 }
	$_.Chart = ($BarChar * ($Width * $coeff)).PadRight($Width, $SpaceChar)
}

# format
Format-Table -AutoSize -InputObject $data
