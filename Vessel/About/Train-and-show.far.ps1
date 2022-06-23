<#
.Synopsis
	Train modes 0 and 1 and show test results.
#>

param(
	[string]$Root = "$env:FARLOCALPROFILE\FarNet\Vessel"
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1
Import-Module $PSScriptRoot\Base.psm1

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$res = $(
	if (Test-Path -LiteralPath "$Root\VesselCommands.txt") {
		Get-Train 2 "$Root\VesselCommands.txt"
	}
	if (Test-Path -LiteralPath "$Root\VesselFolders.txt") {
		Get-Train 1 "$Root\VesselFolders.txt"
	}
	if (Test-Path -LiteralPath "$Root\VesselHistory.txt") {
		Get-Train 0 "$Root\VesselHistory.txt"
	}
)
$sw.Stop()

$score = @{n='PScore'; e={Get-Percent $_.Score $_.MaxScore 2}}
$gain = @{n='PGain'; e={Get-Percent $_.Gain $_.MaxGain 2}}

@"
VESSEL TRAINIG: Time: $($sw.Elapsed.TotalSeconds) sec
$(($res | Format-Table Tests, Score, Gain, $score, $gain, UpCount, DownCount | Out-String).Trim())
"@
