<#
.Synopsis
	Test editor drawer with all console colors.

.Description
	This script registers a drawer which gets fixed color collection for files
	named "Colors". Then it creates one such file and opens it in the editor.

	Script uses two variables:

		$this [FarNet.IEditor]
		$_ [FarNet.ModuleDrawerEventArgs]
		.Colors - result color collection
		.Lines - lines to get colors for
		.StartChar - the first character
		.EndChar - after the last character
#>

# Register the drawer
Register-FarDrawer -Mask Colors -Priority 1 'Show text colors' 4ddb64b8-7954-41f0-a93f-d5f6a09cc752 {
	foreach($back in 0..15) {
		foreach($fore in 0..15) {
			$_.Colors.Add((New-Object FarNet.EditorColor $back, ($fore * 3), ($fore * 3 + 3), $fore, $back))
		}
	}
}

# Temp file with text to be shown with colors
[System.IO.File]::WriteAllLines("$env:TEMP\Colors", @(
	foreach($back in 0..15) {
		$line = ''
		foreach($fore in 0..15) {$line += " {0:X} " -f $fore}
		$line + (" {0:X} {0:d2} {1}" -f $back, [ConsoleColor]$back)
	}
))

# Edit the file with shown colours
Open-FarEditor -Path $env:TEMP\Colors -DeleteSource File -IsLocked -DisableHistory
