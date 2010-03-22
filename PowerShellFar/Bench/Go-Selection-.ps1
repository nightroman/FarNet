
<#
.SYNOPSIS
	Goes to the current selection start\end position
	Author: Roman Kuzmin

.DESCRIPTION
	Many popular editors on [Left]\[Right] put cursor to the start\end position
	of the selected text and drop the selection. The script does the same in
	editor, command line and dialog edit boxes.

	NOTE: since Far introduced the macro function Editor.Sel() it is more
	effective for [Left]\[Right] macros than the script.

.LINK
	Profile-.ps1 - how to add a script to the "User menu".
	Install-Macro-.ps1 - how to add macros.
	Help: Autoloaded functions

.PARAMETER End
		Go to selection end.
#>

param
(
	[switch]$End
)

function global:Go-Selection-
(
	[switch]$End
)
{
	if ($Far.Window.Kind -eq 'Editor') {
		$editor = $Far.Editor
		if ($editor.SelectionExists) {
			if ($End) {
				$editor.GoTo($shape.Right + 1, $shape.Bottom)
			}
			else {
				$editor.GoTo($shape.Left, $shape.Top)
			}
			$editor.UnselectText()
		}
	}
	else {
		$line = $Far.Line
		$select = $line.Selection
		if ($line -and $select.Start -ge 0) {
			if ($End) {
				$line.Caret = $select.End
			}
			else {
				$line.Caret = $select.Start
			}
			$line.UnselectText()
		}
	}
}

Go-Selection- -End:$End
