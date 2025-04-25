<#
.Synopsis
	Goes to the current editor selection start/end.

.Description
	Many editors on [Left]/[Right] put cursor to the selection start/end and
	drop the selection. The script does this in the current editor, command
	line, or edit box.

	The script is just a sample. The macro Editor.Sel() is better in practice.
#>

param(
	[switch]$End
)

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
