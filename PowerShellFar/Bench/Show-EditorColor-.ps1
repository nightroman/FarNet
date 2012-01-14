
<#
.Synopsis
	Shows editor color information.
	Author: Roman Kuzmin
#>

$Editor = $Psf.Editor()
for($lineIndex = 0; $lineIndex -lt $Editor.Count; ++$lineIndex) {
	''
	$line = $Editor[$lineIndex]
	$text = $line.Text
	foreach($color in $Editor.GetColors($lineIndex) | Sort-Object Priority, Start, End) {
		if ($color.End -le $text.Length) {
			'{0,-80} : {1}' -f $color, $text.Substring($color.Start, $color.End - $color.Start)
		} #
		else {
			"$color"
		}
	}
}
