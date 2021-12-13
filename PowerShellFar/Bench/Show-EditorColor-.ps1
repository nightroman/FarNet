<#
.Synopsis
	Shows color information for the current editor.
	Author: Roman Kuzmin
#>

Assert-Far -Editor -Message 'Invoke this script from the editor.' -Title 'Show-EditorColor-.ps1'

$Editor = $Far.Editor
$colors = [System.Collections.Generic.List[FarNet.EditorColorInfo]]::new()
foreach($line in $Editor.Lines) {
	''
	$text = $line.Text
	$Editor.GetColors($line.Index, $colors)
	foreach($color in $colors | Sort-Object Priority, Start, End) {
		if ($color.End -le $text.Length) {
			'{0,-80} : {1}' -f $color, $text.Substring($color.Start, $color.End - $color.Start)
		}
		else {
			"$color"
		}
	}
}
