<#
.Synopsis
	Panel PowerShell provider item properties.
	Author: Roman Kuzmin

.Description
	Warning: depending on a provider you can create\delete\edit properties in a
	panel affecting real source data, like registry values, see example.

.Parameter Path
		The item path. If it is wildcard then it must be resolved to one item.

.Parameter LiteralPath
		The existing item literal path.

.Example
	Panel-ItemProperty.ps1 HKCU:\Environment
	View/edit user environment variables.

.Example
	Panel-ItemProperty.ps1 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment'
	View/edit machine environment variables.
#>

[CmdletBinding()]
param(
	[Parameter(ParameterSetName='Path', Mandatory=1, Position=0)]
	$Path
	,
	[Parameter(ParameterSetName='LiteralPath', Mandatory=1)]
	$LiteralPath
)

$ErrorActionPreference = '1'
trap { $PSCmdlet.ThrowTerminatingError($_) }

$item = @(Get-Item -Force @PSBoundParameters)
if (!$item) { throw "No items found." }
if ($item.Count -gt 1) { throw "Expected one item, found $($item.Count)." }

[PowerShellFar.PropertyPanel]::new($item[0].PSPath).Open()
