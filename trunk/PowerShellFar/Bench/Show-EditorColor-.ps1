
<#
.Synopsis
	Shows color information for the current editor.
	Author: Roman Kuzmin
#>

Assert-Far -Editor -Message 'Invoke this script from the editor.' -Title 'Show-EditorColor-.ps1'

$Editor = $Far.Editor
foreach($line in $Editor.Lines) {
	''
	$text = $line.Text
	foreach($color in $Editor.GetColors($line.Index) | Sort-Object Priority, Start, End) {
		if ($color.End -le $text.Length) {
			'{0,-80} : {1}' -f $color, $text.Substring($color.Start, $color.End - $color.Start)
		}
		else {
			"$color"
		}
	}
}
