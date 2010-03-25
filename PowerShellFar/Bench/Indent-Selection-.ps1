
<#
.SYNOPSIS
	Indents or outdents selected lines in the editor
	Author: Roman Kuzmin

.DESCRIPTION
	Increments or decrements indentation of selected lines in editor according
	to current tab settings. Many popular editors do this on [Tab]\[ShiftTab].

.LINK
	Profile-.ps1 - how to add these two commands to the "User menu".
	Install-Macro-.ps1 - how to add [Tab]\[ShiftTab] key macros.
	Help: Autoloaded functions

.PARAMETER Outdent
		Outdent selected lines, i.e. decrement indentation.
#>

param
(
	[switch]$Outdent
)

function global:Indent-Selection- ([switch]$Outdent)
{
	$Editor = $Psf.Editor()
	if ($Editor.SelectionKind -ne 'Stream') {
		return
	}

	$Editor.BeginUndo()

	$tabSize = $Editor.TabSize
	foreach($line in $Editor.SelectedLines) {
		if ($line.SelectionSpan.Length -le 0) {
			continue
		}
		$text = $line.Text
		if ($Outdent) {
			if ($text[0] -eq "`t") {
				$line.Text = $text.Substring(1)
			}
			else {
				for($i = 0; $i -lt $tabSize; ++$i) {
					if ($text[$i] -ne ' ') { break }
				}
				if ($i -lt $text.Length) {
					$line.Text = $text.Substring($i)
				}
			}
		}
		else {
			if ($Editor.ExpandTabs -ne 'None') {
				$line.Text = ' ' * $Editor.TabSize + $text
			}
			else {
				$line.Text = "`t" + $text
			}
		}
	}

	$Editor.EndUndo()
}

Indent-Selection- -Outdent:$Outdent
