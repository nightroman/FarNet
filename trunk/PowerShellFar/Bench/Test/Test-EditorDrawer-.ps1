
<#
.Synopsis
	Test editor drawer with all console colors.
	Author: Roman Kuzmin
#>

$color=[Enum]::GetValues([ConsoleColor])

# Open editor and set text
Open-FarEditor -DeleteSource File $env:TEMP\Colors
$Far.Editor.SetText((.{
	for($back = 0; $back -le 15; ++$back) {
		$line = ''
		for($fore = 0; $fore -le 15; ++$fore) {
			$line += " {0:X} " -f $fore
		}
		$line + (" {0:X} {0:d2} {1}" -f $back, $color[$back])
	}
} | Out-String))

# The drawer gets colors
$getColors = {&{
	$colors = New-Object System.Collections.Generic.List[FarNet.EditorColor]
	for($back = 0; $back -le 15; ++$back) {
		for($fore = 0; $fore -le 15; ++$fore) {
			$colors.Add((New-Object FarNet.EditorColor $back, ($fore * 3), ($fore * 3 + 3), $fore, $back))
		}
	}
	, $colors
}}

# Register the drawer and redraw
$Far.Editor.RegisterDrawer((New-Object FarNet.EditorDrawer $getColors, "4ddb64b8-7954-41f0-a93f-d5f6a09cc752", 1))
$Far.Editor.Redraw()
