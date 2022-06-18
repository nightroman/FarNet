<#
.Synopsis
	Train modes 0 and 1 and show test results.
#>

Set-StrictMode -Version 3
$ErrorActionPreference = 1
Import-Module $PSScriptRoot\Base.psm1

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$res = $(
	Get-Train 2
	Get-Train 1
	Get-Train 0
)
$sw.Stop()

@"
VESSEL TRAINIG: Time: $($sw.Elapsed.TotalSeconds) sec
$(($res | Format-Table Average, UpCount, DownCount, SameCount | Out-String).Trim())
"@
