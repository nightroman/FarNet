<#
.Synopsis
	Invokes merge for two panels items.
	Author: Roman Kuzmin

.Description
	The merge command is defined by the environment variable MERGE.
	This command takes two arguments, the paths to merge.

	Target items in the active and passive panels:
	- two selected active items
	- selected active and selected passive items
	- selected active and its mirror if passive is dots

	The target items must be both files or folders.
#>

try {
	if (!$env:MERGE) { throw 'Expected environment variable MERGE.' }

	$a, $b, $c = Get-FarPath -Selected
	if (!$a) { throw 'No selected items in the active panel.' }
	if ($c) { throw 'Too many selected items in the active panel.' }

	if (!$b) {
		$b, $c = Get-FarPath -Selected -Passive
		if ($c) { throw 'Too many selected items in the passive panel.' }

		if (!$b) {
			$b = Get-FarPath -Selected -Mirror
			if (!$b) { throw 'Cannot find target items.' }
		}
	}

	$itemA = Get-Item -LiteralPath $a
	$itemB = Get-Item -LiteralPath $b
	if ($itemA.GetType() -ne $itemB.GetType()) { throw 'Incompatible selected items.' }

	& $env:MERGE $a $b
}
catch {
	Show-FarMessage $_ -Caption Merge-Selected
}
