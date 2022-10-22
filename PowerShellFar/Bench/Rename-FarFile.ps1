<#
.Synopsis
	Renames the current panel file or directory.
	Author: Roman Kuzmin

.Description
	Requires: https://www.powershellgallery.com/packages/FarDescription

	The script renames the current file panel item, updates its description,
	and sets the renamed item current in the panel. By default it prompts to
	confirm or change the new name.

.Parameter Name
		New name, either [string] or [scriptblock] with $_ = FileSystemInfo.

.Parameter Quiet
		Tells to rename without confirmation.

.Example
	> Rename-FarFile { $_.LastWriteTime.ToString('_yyMMdd_HHmmss_') + $_.Name }

	Add a prefix based on LastWriteTime.
#>

param(
	[Parameter(Position=0, Mandatory=1)]
	[object]$Name
	,
	[switch]$Quiet
)

$ErrorActionPreference=1
Import-Module FarDescription

### get existing FileSystemInfo item
$private:item = $Far.FS.CursorItem
if (!$item) {
	return
}

### get the new name
if ($Name -is [scriptblock]) {
	$_ = $item
	$newName = & $Name
}
else {
	$newName = $Name
}

### ask
if (!$Quiet) {
	$newName = $Far.Input('New name', $null, "Rename $($item.Name)", $newName)
	if (!$newName) {
		return
	}
}

### rename using FarMoveTo(), so that the description gets updated
$path = Join-Path ([System.IO.Path]::GetDirectoryName($item.FullName)) $newName
$item.FarMoveTo($path)

### update the panel and go to the renamed file
$Far.Panel.Update($true)
$Far.Panel.GoToName($newName)
