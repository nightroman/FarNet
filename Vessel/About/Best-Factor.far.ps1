<#
.Synopsis
	Finds the best Factor.
#>

param(
	[string]$Root,
	$FactorName = 'FactorSpanMin',
	$TryFactors = @(0.4; 0.5; 0.6; 0.7; 0.8)
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1

Import-Module $PSScriptRoot\Base.psm1
$Factor = [Vessel.Info]::$FactorName
$TryFactors = @($TryFactors; $Factor) | Sort-Object -Unique

function Get-TrainWithFactor($Mode, $Path, $Factor) {
	$old = [Vessel.Info]::$FactorName
	[Vessel.Info]::$FactorName = $Factor
	try {
		Get-Train $Mode $Path
	}
	finally {
		[Vessel.Info]::$FactorName = $old
	}
}

function Test-Mode($Mode, $Path) {
	$tests = foreach($factor in $TryFactors) {
		$r = Get-TrainWithFactor $Mode $Path $factor
		[PSCustomObject]@{
			Factor = $factor
			Tests = $r.Tests
			Score = $r.Score
			Gain = $r.Gain
			PScore = Get-Percent $r.Score $r.MaxScore 2
			PGain = Get-Percent $r.Gain $r.MaxGain 2
			UpCount = $r.UpCount
			DownCount = $r.DownCount
		}
	}
	$tests | Sort-Object Score, Gain, @{e={$_.Factor}; d=$true}
}

function Show-Mode($Mode, $Path) {
	if ($Path -and !(Test-Path -LiteralPath $Path)) {
		return Write-Warning "Missing '$Path'."
	}
	$res = Test-Mode $Mode $Path
	@"
Vessel Mode=$Mode
$(($res | Format-Table | Out-String).Trim())
"@
	$current = $res.where{ $_.Factor -eq $Factor }
	'*{0} {1}%' -f @(
		$Factor
		Get-Difference $res[-1].Score $current.Score
	)
}

'*' * 77

Show-Mode 2 $(if ($Root) {"$Root\VesselCommands.txt"})
Show-Mode 1 $(if ($Root) {"$Root\VesselFolders.txt"})
Show-Mode 0 $(if ($Root) {"$Root\VesselHistory.txt"})
