
<#
.Synopsis
	Test editor drawer with all console colors.
	Author: Roman Kuzmin

.Link
	Draw-Word-.ps1
#>

# Open editor and set text
Open-FarEditor -DeleteSource File $env:TEMP\Colors
$color = [Enum]::GetValues([ConsoleColor])
$Far.Editor.SetText((.{
	for($back = 0; $back -le 15; ++$back) {
		$line = ''
		for($fore = 0; $fore -le 15; ++$fore) {
			$line += " {0:X} " -f $fore
		}
		$line + (" {0:X} {0:d2} {1}" -f $back, $color[$back])
	}
} | Out-String))

# Register the drawer
$Far.Editor.RegisterDrawer((New-Object FarNet.EditorDrawer '4ddb64b8-7954-41f0-a93f-d5f6a09cc752', 1, {param($Editor, $Colors) &{
	# Fill the color collection. This sample is trivial, it returns fixed
	# colors. For more practical example see the script Draw-Word-.ps1.
	for($back = 0; $back -le 15; ++$back) {
		for($fore = 0; $fore -le 15; ++$fore) {
			$Colors.Add((New-Object FarNet.EditorColor $back, ($fore * 3), ($fore * 3 + 3), $fore, $back))
		}
	}
}}))

# Show right now
$Far.Editor.Redraw()
