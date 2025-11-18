<#
.Synopsis
	Gets versions of Far, FarNet, PowerShell, PowerShellFar.
	Author: Roman Kuzmin
#>

#requires -Version 7.4
$ErrorActionPreference=1; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

if ((Get-Item $env:FARHOME\FarNet\FarNet.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host FN=DEBUG -ForegroundColor Red }
if ((Get-Item $env:FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host PS=DEBUG -ForegroundColor Red }

[ordered]@{
	Far = $Far.FarVersion
	FarNet = $Far.FarNetVersion
	PowerShell = $PSVersionTable.PSVersion
	PowerShellFar = $Host.Version
}
