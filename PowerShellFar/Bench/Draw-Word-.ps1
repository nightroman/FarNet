
<#
.Synopsis
	Draws occurrences of the current word the editor.
	Author: Roman Kuzmin
#>

Assert-Far -Editor -Message 'Invoke this script from the editor.' -Title 'Highlight-Word-.ps1'

$GetColors = { param($Editor, $Colors, $StartLine, $EndLine) &{
	$regex = [regex]'\w[-\w]*'

	# get the word
	$line = $Editor.Line
	$char = $line.Caret
	foreach($match in $regex.Matches($line.Text)) {
		if ($char -le ($match.Index + $match.Length)) {
			if ($char -lt $match.Index) { return }
			break
		}
	}
	if (!$match.Success) { return }
	$word = $match.Value

	# color the word
	for($1 = $StartLine; $1 -lt $EndLine; ++$1) {
		foreach($match in $regex.Matches($Editor[$1].Text)) {
			if ($word -eq $match) {
				$Colors.Add((New-Object FarNet.EditorColor $1, $match.Index, ($match.Index + $match.Length), 'Black', 'Gray'))
			}
		}
	}
}}

$Far.Editor.RegisterDrawer((New-Object FarNet.EditorDrawer $GetColors, '52f52a6e-ff02-477c-ac78-a5d172bb569d', 1))
$Far.Editor.Redraw()
