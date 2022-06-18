<#
.Synopsis
	Makes CSV of file openings with idle times.

.Description
	CSV is used by other scripts for charts.
#>

param(
	[Parameter()]
	[string]$Path = "$env:FARLOCALPROFILE\FarNet\Vessel"
	,
	[string]$OutputCsv = "$env:TEMP\z.VesselIdle.csv"
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1

$map = @{}
$(
	Import-Csv "$Path\VesselFolders.txt" -Delimiter "`t"
	Import-Csv "$Path\VesselHistory.txt" -Delimiter "`t"
) |
Sort-Object Time |
.{process{
	$name = $_.Path
	$time = [DateTime]$_.Time
	$last = $map[$name]
	if ($last) {
		$idle = [Math]::Log(($time - $last).TotalHours, 2)
		if ($idle -ge 0) {
			[PSCustomObject]@{
				Path = $name
				Idle = $idle
			}
		}
	}
	$map[$name] = $time
}} |
Export-Csv $OutputCsv -NoTypeInformation
