<#
.Synopsis
	Test-TabExpansion2-.ps1 in pwsh and powershell.
#>

[CmdletBinding()]
param(
	[ValidateSet('pwsh', 'powershell')]
	[string]$Shell = 'pwsh'
	,
	[switch]$NoExit
)

$ErrorActionPreference = 1

$oldPSModulePath = $env:PSModulePath
$env:PSModulePath = [Environment]::GetEnvironmentVariable('PSModulePath', 'user')
try {
	$param = @(
		if ($NoExit) {
			'-NoExit'
		}
		'-NoProfile',
		'-Command',
		'.\Test-TabExpansion2-.ps1'
	)
	$process = Start-Process $Shell -ArgumentList $param -WorkingDirectory $PSScriptRoot -PassThru -Wait:(!$NoExit)
	if (!$NoExit -and $process.ExitCode) {
		throw "Test TabExpansion2 failed in $Shell."
	}
}
finally {
	$env:PSModulePath = $oldPSModulePath
}
