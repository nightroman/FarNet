<#
.Synopsis
	Test-TabExpansion2.far.ps1 in pwsh and powershell.
#>

[CmdletBinding()]
param(
	[ValidateSet('pwsh', 'powershell')]
	[string]$pwsh = 'pwsh'
	,
	[switch]$NoExit
)

$ErrorActionPreference=1

$PSModulePath = $env:PSModulePath
$env:PSModulePath = [Environment]::GetEnvironmentVariable('PSModulePath', 'Machine')
try {
	$param = @(
		if ($NoExit) {
			'-NoExit'
		}
		'-NoProfile',
		'-Command',
		'.\Test-TabExpansion2.far.ps1'
	)
	$process = Start-Process $pwsh -ArgumentList $param -WorkingDirectory $PSScriptRoot -PassThru -Wait:(!$NoExit)
	if (!$NoExit -and $process.ExitCode) {
		throw "Test TabExpansion2 failed in $pwsh."
	}
}
finally {
	$env:PSModulePath = $PSModulePath
}
