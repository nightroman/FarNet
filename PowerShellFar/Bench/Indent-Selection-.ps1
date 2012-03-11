
<#
.Synopsis
	Indents selected lines in the editor.
	Author: Roman Kuzmin

.Description
	Increments or decrements indentation of selected lines in editor according
	to current tab settings. Many popular editors do this on [Tab]/[ShiftTab].

.Parameter Prefix
		Indentation prefix. The default is "`t" which takes into account the
		current tab expansion mode. Any other prefix is treated literally, it
		is added or removed.

.Parameter Back
		Indent selected lines back (decrement indentation, remove a prefix).
#>

param
(
	[Parameter()][string]$Prefix,
	[switch]$Back
)

$Editor = $Psf.Editor()
if (!$Editor.SelectionExists) {return}

$tab = if ($Prefix) {
	$Prefix
	$tab2 = $Prefix.TrimEnd()
}
else {
	"`t"
	$TabSize = $Editor.TabSize
}

$Editor.BeginUndo()
foreach($line in $Editor.SelectedLines) {
	if (!$Prefix -and $line.SelectionSpan.Length -le 0) {
		continue
	}
	$text = $line.Text
	if ($Back) {
		if ($text.StartsWith($tab)) {
			$line.Text = $text.Substring($tab.Length)
		}
		elseif (!$Prefix) {
			for($i = 0; $i -lt $TabSize; ++$i) {
				if ($text[$i] -ne ' ') { break }
			}
			if ($i -lt $text.Length) {
				$line.Text = $text.Substring($i)
			}
		}
		elseif ($text.StartsWith($tab2)) {
			$line.Text = $text.Substring($tab2.Length)
		}
	}
	else {
		if (!$Prefix -and $Editor.ExpandTabs -ne 'None') {
			$line.Text = ' ' * $TabSize + $text
		}
		else {
			$line.Text = $tab + $text
		}
	}
}
$Editor.EndUndo()
