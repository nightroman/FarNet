<#
.Synopsis
	Edits scripts and functions found by Get-Command.
	Author: Roman Kuzmin

.Description
	It finds available matches and shows the result list.
	The selected script or function is opened in editor.

.Parameter Command
		Used as `Get-Command -Command`, supports wildcards.
		Default is "*".

.Example
	>
	Find and show scripts like "panel*"

		ps: Edit-Gcm.ps1 panel*

.Example
	>
	The same using Lua macro command "gcm:"

		gcm: panel*

	Macro command "gcm:"

		CommandLine {
		  prefixes = "gcm";
		  description = "Edit-Gcm.ps1";
		  action = function(prefix, text)
		    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:Edit-Gcm.ps1 " .. text)
		  end;
		}
#>

[CmdletBinding()]
param(
	[ValidateNotNullOrEmpty()]
	[string]$Command = '*'
)

Set-StrictMode -Version 3
#requires -Version 7.4
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$resolve_alias = {
	param($cmd)
	while($cmd.CommandType -eq 'Alias') {
		$definition = $cmd.Definition
		if (!$definition) {
			return
		}
		$cmd = Get-Command $definition -ErrorAction Ignore
		if (!$cmd) {
			return
		}
	}
	$cmd
}

### List
$list = @(
	foreach($cmd in Get-Command $Command -CommandType 'Alias,ExternalScript,Application,Function' -ErrorAction Ignore) {
		### Alias
		$cmd = & $resolve_alias $cmd
		if (!$cmd) {
			continue
		}

		### ExternalScript
		if ($cmd.CommandType -eq 'ExternalScript') {
			$cmd.Path
			continue
		}

		### Application
		if ($cmd.CommandType -eq 'Application') {
			$path = $cmd.Path
			if ($path -notmatch '\.(?:exe|com|cpl|msc)$') {
				$path
			}
			continue
		}

		### Function
		if ($cmd.CommandType -eq 'Function') {
			$path = $cmd.ScriptBlock.File
			if ($path) {
				$cmd
			}
			continue
		}
	}
)

### Show
if (!$list) {
	return
}
if ($list.Count -eq 1) {
	$item = $list[0]
}
else {
	$get_text = {
		$_ -is [string] ? $_ : "$($_.ScriptBlock.File) function $($_.Name)"
	}
	$list = $list | Sort-Object $get_text -Unique
	$item = $list | Out-FarList -Title "Command: $Command" -Text $get_text
	if (!$item) {
		return
	}
}

### Edit
if ($item -is [string]) {
	Open-FarEditor $item
}
else {
	Open-FarEditor $item.ScriptBlock.File -LineNumber $item.ScriptBlock.StartPosition.StartLine
}
