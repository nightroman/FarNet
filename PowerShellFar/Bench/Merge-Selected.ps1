<#
.Synopsis
	Invokes merge for two panels items.
	Author: Roman Kuzmin

.Description
	Target items in the active and passive panels:
	- two selected active items
	- selected active and selected passive items
	- selected active and its mirror if passive is dots

	The target items must be both files or folders.

.Parameter Tool
		Specifies the diff tool name: VSCode, WinMerge.

		Default is $env:MERGE. It should define a command called with two
		arguments, items to diff.
#>

[CmdletBinding()]
param(
	[ValidateSet('VSCode', 'WinMerge')]
	[string]$Tool = $env:MERGE
)

trap { $PSCmdlet.ThrowTerminatingError($_) }

$a, $b, $c = Get-FarPath -Selected
if (!$a) { throw 'No selected items in the active panel.' }
if ($c) { throw 'Too many selected items in the active panel.' }

if (!$b) {
	$b, $c = Get-FarPath -Selected -Passive
	if ($c) { throw 'Too many selected items in the passive panel.' }

	if (!$b) {
		$b = Get-FarPath -Selected -Mirror
		if (!$b) { throw 'Cannot find the second target item.' }
	}
}

$itemA = Get-Item -LiteralPath $a
$itemB = Get-Item -LiteralPath $b -ErrorAction Ignore
if (!$itemB) { throw 'Cannot find the second target item.' }
if ($itemA.GetType() -ne $itemB.GetType()) { throw 'Incompatible selected items.' }

switch($Tool) {
	VSCode {
		code --diff $a $b
	}
	WinMerge {
		& "$env:ProgramFiles\WinMerge\WinMergeU.exe" $a $b
	}
	default {
		if (!$Tool) { throw 'Specify Tool or define $env:MERGE.' }
		& $Tool $a $b
	}
}
