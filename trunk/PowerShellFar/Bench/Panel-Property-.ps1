
<#
.SYNOPSIS
	Panel PowerShell provider item properties.
	Author: Roman Kuzmin

.DESCRIPTION
	In case of several items only the first one is processed.

	Warning: depending on a provider you can create\delete\edit properties in a
	panel affecting real source data, like registry values, see example.

.EXAMPLE
	Panel-Property- HKCU:\Soft*\Far2\*\PowerShellFarHistory
#>

param
(
	# Path of an item which properties are shown.
	$Path,

	# Literal path of an item.
	$LiteralPath
)

if ($Path) {
	$item = @(Get-Item -Path $Path -Force)
}
elseif ($LiteralPath) {
	$item = @(Get-Item -LiteralPath $Path -Force)
}
else {
	$item = @($input)
	if (!$item) { throw "Missed input, -Path and -LiteralPath." }
}
if (!$item) { throw "No items found." }

(New-Object PowerShellFar.PropertyPanel $item[0].PSPath).Open()
