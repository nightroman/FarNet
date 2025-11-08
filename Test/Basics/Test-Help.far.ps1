<#
.Synopsis
	Tests and builds PS help.
#>

param([switch]$Force)

. Library-LastWriteTime.ps1

### test up-to-date

$helpScript = "$env:FarNetCode\PowerShellFar\Commands\Help.ps1"
$helpOutput = "$($Psf.AppHome)\PowerShellFar.dll-Help.xml"

$toBuild = $Force -or (!(Test-Path $helpOutput))
if (!$toBuild) {
	$timeSource = Get-Item $helpScript, C:\ROM\APS\Do\Helps\*.*, $env:FarNetCode\PowerShellFar\Commands\*.cs | Get-LastWriteTimeMaximum
	$timeOutput = (Get-Item $helpOutput).LastWriteTime
	$toBuild = $timeSource -gt $timeOutput
	@"
Source : $timeSource
Output : $timeOutput
Update : $toBuild
"@
}

### build help

if ($toBuild) {
	Write-Host Help is outdated. Building...
	. Helps.ps1
	Convert-Helps $helpScript $helpOutput
	Assert-Far (Test-Path $helpOutput)
}

### test synopsis for each *-Far* cmdlet

Get-Command *-Far* -CommandType cmdlet | Get-Help | .{process{
	$lines = $_.Synopsis -split '\r?\n'
	if (!$lines) {throw 'Empty synopsis.'}
	if (!$lines[0].EndsWith('.')) {throw "Unexpected synopsis in $($_.Name)"}
}}
