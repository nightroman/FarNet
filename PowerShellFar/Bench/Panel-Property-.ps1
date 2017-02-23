
<#
.Synopsis
	Panel PowerShell provider item properties.
	Author: Roman Kuzmin

.Description
	In case of several items only the first is processed.

	Warning: depending on a provider you can create\delete\edit properties in a
	panel affecting real source data, like registry values, see example.

.Parameter Path
		Path of an item which properties are shown.
.Parameter LiteralPath
		Literal path of an item.

.Example
	Panel-Property-.ps1 HKCU:\Environment
	View/edit user environment variables.

.Example
	Panel-Property-.ps1 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment'
	View/edit machine environment variables.
#>

[CmdletBinding()] param(
	$Path,
	$LiteralPath
)

$ErrorActionPreference = 'Stop'

$item = @(Get-Item -Force @PSBoundParameters)
if (!$item) { Write-Error "No items found." }

(New-Object PowerShellFar.PropertyPanel $item[0].PSPath).Open()
