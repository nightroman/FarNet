<#
.Synopsis
	Re-indents selected lines or the current line.
	Author: Roman Kuzmin

.Description
	Designed for formatting indentations in PowerShell-like code.

	Indentation is affected by leading and trailing {}() in lines. Lines in
	here-strings @".."@, @'..'@ and simple block-comments are not changed.
#>

[CmdletBinding()]
param()

#requires -Version 7.4
$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$Editor = $Psf.Editor()

# $n1, $n2 - first (unchanged) and last lines to process
$place = $Editor.SelectionPlace
if ($place.Top -lt 0) {
	$n2 = $Editor.Caret.Y
	$n1 = $n2 - 1
	if ($n1 -lt 0) { return }
}
else {
	$n2 = $place.Bottom
	$n1 = $place.Top
	if ($n1 -gt 0) { --$n1 }
}

# find indent at the first solid line
$found = $false
for($n = $n1) {
	$text = $Editor[$n].Text
	if ($text -match '^(\s*)\S') {
		$found = $true
		$indent = $matches[1]
		break
	}
	if (--$n -lt 0) { break }
}

# tabs
$ExpandTabs = $Editor.ExpandTabs -ne 'None'
$TabSize = $Editor.TabSize
$add = if ($ExpandTabs) { ' ' * $TabSize } else { "`t" }

# begin
$Editor.BeginUndo()

# selected lines
$mode = 0
:lines
for($n = $n1; $n -le $n2; ++$n)
{
	$line = $Editor[$n]
	$text = $line.Text

	if (!$found) {
		if ($text -notmatch '^(\s*)\S') {
			continue
		}
		$found = $true
		$indent = $Matches[1]
	}

	if ($text -notmatch '^\s*(.*?)(\S)(?:\s*$|(\s*#.*)$)') {
		continue
	}
	$body = $Matches[1]
	$tail = $Matches[2]
	$rem = $Matches[3]
	if ($body) {
		$head = $body[0]
	}
	else {
		$head = $tail
	}

	switch($mode) {
		1 {
			if ($text.StartsWith("'@")) { $mode = 0 }
			continue lines
		}
		2 {
			if ($text.StartsWith('"@')) { $mode = 0 }
			continue lines
		}
		3 {
			if ($text.StartsWith('#>')) { $mode = 0 }
			continue lines
		}
		default {
			if ($tail -eq "'" -and $body.EndsWith('@')) { $mode = 1 }
			elseif ($tail -eq '"' -and $body.EndsWith('@')) { $mode = 2 }
			elseif ($tail -eq '#' -and $body.EndsWith('<')) { $mode = 3 }
		}
	}

	if ($n -gt $n1) {
		if ('}' -eq $head -or ')' -eq $head) {
			if ($indent[0] -eq "`t") {
				$indent = $indent.Substring(1)
			}
			else {
				for($i = 0; $i -lt $TabSize; ++$i) {
					if ($indent[$i] -ne ' ') { break }
				}
				if ($i -le $indent.Length) {
					$indent = $indent.Substring($i)
				}
			}
		}
		$line.Text = $indent + $body + $tail + $rem
	}

	if ('{' -eq $tail -or '(' -eq $tail) {
		$indent += $add
	}
}

# end
$Editor.EndUndo()
