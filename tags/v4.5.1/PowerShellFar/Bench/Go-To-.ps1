
<#
.SYNOPSIS
	Goes to the specified file system item or a provider container
	Author: Roman Kuzmin

.DESCRIPTION
	It is similar to far:goto command but the input path is any suitable
	PowerShell expression. Wildcards resolved to one or more items are
	permitted.

	In addition we can use other PowerShell provider paths, especially Registry
	can be useful. In this case a provider panel is opened.

.EXAMPLE
	# Let's use an alias
	Set-Alias go Go-To-.ps1

	# Go to a special folder
	go *

	# Open the PowerShellFar home
	go $env:farhome\*\powersh*\

	# Go to a plugin help selected from a list
	go $env:farhome\plugins\*\*.hlf

	# Others provider (Registry)
	go registry::*
	go hkcu:\software\far2\key*\shell
	go HKEY_CURRENT_USER\Software\Far

	# Prompts to enter a path (or select from history)
	go

.PARAMETER Path
		*) File system directory or file path. For a directory end \ or / tells
		to open it, not set a cursor to it. Wildcard patterns are permitted. If
		a pattern is resolved to more than one item a list is shown for choice.
		*) If '*' is used as a path parameter then you are prompted to select a
		special system folder from a list.
		*) Other provider path, e.g. registry key path. Only container or root
		paths are permitted, not leaves. The script opens a provider panel with
		child items of a specified container.
#>

param
(
	[Parameter(Mandatory = $true)]
	[string]$Path
)

### Special folder
if ($Path -eq '*') {
	[enum]::GetNames([System.Environment+SpecialFolder]) | Sort-Object | Out-FarList | .{process{
		$Far.Panel.GoToPath([Environment]::GetFolderPath($_) + '\')
	}}
	return
}

### One existing FileSystem item
if ([IO.File]::Exists($Path) -or [IO.Directory]::Exists($Path)) {
	$Far.Panel.GoToPath($Path)
	return
}

### Get items, convert some paths
if ($Path -like 'HKEY_*') { $Path = 'Registry::' + $Path }
$paths = @(Resolve-Path $Path -ErrorAction 0)
if (!$paths) { return }
$items = @(Get-Item -LiteralPath $paths -Force -ErrorAction 0)
if (!$items) { return }

### FileSystem items
if ($items[0].PSProvider.Name -eq 'FileSystem') {

	if ($items.Count -eq 1) {
		$item = $items[0]
	}
	else {
		$item = $items | Out-FarList -Title 'Go to' -Text 'Name'
		if (!$item) {
			return
		}
	}

	if ($Path.EndsWith('\') -or $Path.EndsWith('/')) {
		$Far.Panel.GoToPath($item.FullName + '\')
	}
	else {
		$Far.Panel.GoToPath($item.FullName)
	}
}
### Provider items
else {
	if ($paths.Count -eq 1) {
		$thePath = $paths[0]
	}
	else {
		$thePath = $paths | Out-FarList -Title 'Go to'
		if (!$thePath) { return }
	}
	New-Object PowerShellFar.ItemPanel $thePath | Open-FarPanel
}
