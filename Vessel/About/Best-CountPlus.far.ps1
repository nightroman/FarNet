<#
.Synopsis
	Finds the best CountPlus
#>

param(
	[string]$Root
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1

Import-Module $PSScriptRoot\Base.psm1
$CountPlus = [Vessel.Info]::CountPlus

function Get-TrainCountPlus($Mode, $Path, $CountPlus) {
	$old = [Vessel.Info]::CountPlus
	[Vessel.Info]::CountPlus = $CountPlus
	try {
		Get-Train $Mode $Path
	}
	finally {
		[Vessel.Info]::CountPlus = $old
	}
}

function Test-Mode($Mode, $Path) {
	$tests = foreach($plus in 0..5) {
		$r = Get-TrainCountPlus $Mode $Path $plus
		[pscustomobject]@{
			Plus = $plus
			Average = [Math]::Round($r.Average, 6)
			UpCount = $r.UpCount
			DownCount = $r.DownCount
			SameCount = $r.SameCount
		}
	}
	$tests | Sort-Object Average, @{e={$_.Plus}; desc=$true}
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
	$current = $res.where{ $_.Plus -eq $CountPlus }
	if ($current.Average -eq $res[-1].Average) {
		'  ({0}) wins' -f $CountPlus
	}
	else {
		'  ({0}) loses ~ {1:p2}' -f $CountPlus, (($res[-1].Average - $current.Average) / $res[-1].Average)
	}
}

'
*****************************************
'

Show-Mode 2 $(if ($Root) {"$Root\VesselCommands.txt"})
Show-Mode 1 $(if ($Root) {"$Root\VesselFolders.txt"})
Show-Mode 0 $(if ($Root) {"$Root\VesselHistory.txt"})
