
<#
.SYNOPSIS
	Goes to the current selection start\end position
	Author: Roman Kuzmin

.DESCRIPTION
	Many popular editors on [Left]\[Right] put cursor to the start\end position
	of the selected text and drop the selection. The script does the same in
	editor, command line and dialog edit boxes.

	NOTE: since Far introduced the macro function Editor.Sel() macros are more
	effective for this job. The script is kept as an example and for cases when
	macros do not work or cannot be called.

.LINK
	Profile-.ps1 - how to add a script to the "User menu".
	Install-Macro-.ps1 - how to add macros.
	Help: Autoloaded functions
#>

param
(
	[switch]
	# Tells to go to the selection end.
	$End
)

function global:Go-Selection-
(
	[switch]$End
)
{
	if ($Far.Window.Kind -eq 'Editor') {
		$Editor = $Far.Editor
		$Place = $Editor.SelectionPlace
		if ($Place.Top -ge 0) {
			if ($End) {
				$Editor.GoTo($Place.Right + 1, $Place.Bottom)
			}
			else {
				$Editor.GoTo($Place.Left, $Place.Top)
			}
			$Editor.UnselectText()
		}
	}
	else {
		$Line = $Far.Line
		if ($Line) {
			$span = $Line.SelectionSpan
			if ($span.Start -ge 0) {
				if ($End) {
					$Line.Caret = $span.End
				}
				else {
					$Line.Caret = $span.Start
				}
				$Line.UnselectText()
			}
		}
	}
}

Go-Selection- -End:$End
