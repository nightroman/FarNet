
<#
.SYNOPSIS
	Panel Windows shortcut properties.
	Author: Roman Kuzmin

.DESCRIPTION
	Shows Windows shortcut properties in a panel. For now changes are not
	saved, so that it is "view only". If saving is needed, please, request.

	It also gives an idea how to create or change shortcuts:
	-- create or open a shortcut by CreateShortcut()
	-- change the shortcut properties
	-- call Save()
#>

param
(
	# Shortcut path.
	[string]$Path = (Get-FarPath)
)

$WshShell = New-Object -ComObject 'WScript.Shell'
$shortcut = $WshShell.CreateShortcut($Path)
Start-FarPanel -InputObject $shortcut -Title $Path
