
<#
.Synopsis
	Test editor drawer with all console colors.
	Author: Roman Kuzmin

.Description
	This script registers a drawer which gets fixed color collection for files
	named "Colors". Then it creates one such file and opens it in the editor.
	The second call of the same script removes the drawer; this is only for
	testing, normally drawers should not be unregistered.

	RegisterModuleDrawer() provides an id, attributes (name, mask, priority)
	and a script that uses two automatic variables:
	* $this - [FarNet.IEditor] - current editor
	* $_ - [FarNet.ModuleDrawerEventArgs]:
	* - Colors - result color collection
	* - Lines - lines to get colors for
	* - StartChar - the first character
	* - EndChar - after the last character
#>

# Unregister the drawer if it is already registered and return
$drawer = $Far.GetModuleAction('4ddb64b8-7954-41f0-a93f-d5f6a09cc752')
if ($drawer) {
	Show-FarMessage 'The drawer has been unregistered.'
	$drawer.Unregister()
	return
}

# Register the drawer (id, attributes, and script)
$drawer = $Psf.Manager.RegisterModuleDrawer(
	'4ddb64b8-7954-41f0-a93f-d5f6a09cc752',
	(New-Object FarNet.ModuleDrawerAttribute -Property @{Name = 'Fixed colors'; Mask = 'Colors'; Priority = 1}),
	{&{
		foreach($back in 0..15) {
			foreach($fore in 0..15) {
				$_.Colors.Add((New-Object FarNet.EditorColor $back, ($fore * 3), ($fore * 3 + 3), $fore, $back))
			}
		}
	}}
)

# Make the temp file with the special text to be coloured on opening in the editor
.{
	foreach($back in 0..15) {
		$line = ''
		foreach($fore in 0..15) {$line += " {0:X} " -f $fore}
		$line + (" {0:X} {0:d2} {1}" -f $back, [ConsoleColor]$back)
	}
} | Set-Content -LiteralPath $env:TEMP\Colors

# Open the editor, it will show the text with colours due to the registered drawer
$Editor = New-FarEditor -Path $env:TEMP\Colors -DeleteSource File -IsLocked
$Editor.Open()
