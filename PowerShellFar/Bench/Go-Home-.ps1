
<#
.SYNOPSIS
	Goes to or selects to extended home position in the editor
	Author: Roman Kuzmin

.DESCRIPTION
	The script implements "extended home" behaviour popular in many editors:
	[Home] puts cursor to the first not space or tab position in the current
	line. Similarly, [ShiftHome] selects up to the first solid position from
	the current.

.LINK
	Profile-.ps1 - how to add these two commands to the "User menu".
	Install-Macro-.ps1 - how to add [Home]\[ShiftHome] key macros.
	Help: Autoloaded functions

.PARAMETER Select
		To select to extended home position.
#>

param
(
	[switch]$Select
)

function global:Go-Home-
(
	[switch]$Select
)
{
	$Editor = $Psf.Editor()
	$line = $Editor.CurrentLine
	$text = $line.Text
	if ($text -match '^(\s+)') {
		$pos = $matches[1].Length
		if ($pos -eq $line.Pos) {
			$pos = 0
		}
	}
	else {
		$pos = 0
	}

	if ($Select) {
		$end = $line.Pos
		if ($end -gt 0) {
			if ($line.Selection.Start -eq $pos) {
				$pos = 0
			}
			$line.Select($pos, $end)
		}
		else {
			$line.Select($end, $pos)
		}
	}
	else {
		$line.Unselect()
		$line.Pos = $pos
	}
}

Go-Home- -Select:$Select