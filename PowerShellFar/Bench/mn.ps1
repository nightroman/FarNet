<#
.Synopsis
	Shows command full help with paging when needed.
	Author: Roman Kuzmin

.Description
	It works in ConsoleHost as `help <command> -Full`.
	And it works in FarHost, with some extra features.

	`mn` is after `man`, another PS help command.
	`gh` (after Get-Help) is taken by GitHub CLI.

.Parameter Command
		Command name, required in ConsoleHost.

		May be omitted:
		- in Far panel with .ps1 cursor
		- in Far editor with .ps1 file

.Parameter Anything
		Ignores other parameters, so we can just `mn` before already typed
		commands, possibly with parameters to be ignored.

.Example
	>
	# Using Lua macro command prefix "mn:"

		CommandLine {
		  prefixes = "mn";
		  description = "mn.ps1";
		  action = function(prefix, text)
		    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:mn.ps1 " .. text)
		  end;
		}

	 show the panel .ps1 file help:

	  	mn:

	 or PowerShellFar cmdlet help:

	 	mn: Open-FarEditor
#>

[CmdletBinding()]
param(
	[string]$Command
	,
	[Parameter(ValueFromRemainingArguments)]
	$Anything
)

$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}

if (!$Command) {
	if ($Host.Name -ne 'FarHost') {
		throw 'Requires command name.'
	}

	if ($Far.Window.Kind -eq 'Panels' -and ($$ = Get-FarPath) -like '*.ps1') {
		return help $$ -Full
	}

	if ($Far.Window.Kind -eq 'Editor' -and  ($$ = $Far.Editor.FileName) -like '*.ps1') {
		$Far.Editor.Save()
		return help $Far.Editor.FileName -Full
	}

	throw 'Requires panel or editor with .ps1 file.'
}

# resolve aliases
$cmd = Get-Command $Command
if ($cmd.CommandType -eq 'Alias') {
	$Command = $cmd.Definition
}

help -Full $Command

# loop to prevent premature closing of console with help printed to the end
if ($Host.Name -eq 'ConsoleHost' -and @(Get-History -Count 2).Count -le 1) {
	while(!(Read-Host)){}
}
