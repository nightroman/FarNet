
<#
.SYNOPSIS
	Renames the current panel file or directory.
	Author: Roman Kuzmin

.DESCRIPTION
	The script renames the current file panel item, updates its description,
	and sets the renamed item current in the panel. By default it asks to
	confirm.

.EXAMPLE
	# Add a prefix based on LastWriteTime time:
	Rename-FarFile- { $_.LastWriteTime.ToString('_yyMMdd_HHmmss ') + $_.Name }
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param
(
	# New name. Can be [string] or [scriptblock] with $_ = FileSystemInfo.
	$Name = $(throw "Parameter -Name is missed.")
)

### test the current file and get its FileSystemInfo item, ignore all others
$private:file = Get-FarFile
if (!$file -or $file.Name -eq '..') { return }
$private:path = Get-FarPath
if (![System.IO.File]::Exists($path) -and ![System.IO.Directory]::Exists($path)) { return }
$private:item = Get-Item -LiteralPath $path

### get the new name
if ($Name -is [scriptblock]) {
	$_ = $item
	$newName = & $Name
}
else {
	$newName = $Name
}

### ask
if (!$pscmdlet.ShouldProcess($item.Name, "Rename to '$newName'")) {
	return
}

### rename using FarMoveTo(), so that the description gets updated
$path = Join-Path ([System.IO.Path]::GetDirectoryName($item.FullName)) $newName
$item.FarMoveTo($path)

### update the panel and go to the renamed file
$Far.Panel.Update($true)
$Far.Panel.GoToName($newName)