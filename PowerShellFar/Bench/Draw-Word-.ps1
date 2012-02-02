
<#
.Synopsis
	Draws occurrences of the current word in the editor.
	Author: Roman Kuzmin
#>

Assert-Far -Editor -Message 'Invoke this script from the editor.' -Title 'Highlight-Word-.ps1'

$Far.Editor.RegisterDrawer((New-Object FarNet.EditorDrawer '52f52a6e-ff02-477c-ac78-a5d172bb569d', 1, { param($Editor, $Colors, $Lines) &{
	$regex = [regex]'\w[-\w]*'

	# the current word
	$match = $Editor.Line.MatchCaret($regex)
	if (!$match) {return}
	$word = $match.Value

	# color occurrences
	foreach($line in $Lines) {
		foreach($match in $regex.Matches($line.Text)) {
			if ($word -eq $match) {
				$Colors.Add((New-Object FarNet.EditorColor $line.Index, $match.Index, ($match.Index + $match.Length), 'Black', 'Gray'))
			}
		}
	}
}}))
