
<#
.SYNOPSIS
	Opens a shortcut file (.lnk) in a few ways.
	Author: Roman Kuzmin

.DESCRIPTION
	The script gets a target path from the shortcut file (.lnk). If it is a
	directory path this directory is opened on the current panel. If it is a
	file path the action depends on parameters; default action is to jump to
	the target file, other actions: edit or view the file.

	How to open .lnk files by [Enter], [F3], [F4]:
	Open menu Commands \ File Associations, add an association and set:
	-- Mask: *.lnk
	-- Command for [Enter]: >: Invoke-Shortcut- #
	-- Command for [F3]: >: Invoke-Shortcut- -View #
	-- Command for [F4]: >: Invoke-Shortcut- -Edit #

	[Shift-Enter] still opens shortcut files from Far in standard Windows way.
#>

[CmdletBinding()]
param
(
	# Path of the .lnk file to be opened. Default is the current panel item.
	$Path = (Get-FarPath),

	[switch]
	# To edit the target file in the Far editor.
	$Edit,

	[switch]
	# To view the target file in the Far viewer.
	$View
)

if (![IO.File]::Exists($Path)) {
	return Show-FarMsg "File does not exist: '$Path'"
}

# WMI does not work well with names with spaces, use WScript.Shell.
# Besides, WScript.Shell also works with .url, just in case
$WshShell = New-Object -ComObject WScript.Shell
$link = $WshShell.CreateShortcut([IO.Path]::GetFullPath($Path))
$target = $link.TargetPath
if (!$target) {
	return Show-FarMsg "Cannot get a target path from '$Path'.`nIs it a shortcut file?"
}

if ([IO.Directory]::Exists($target)) {
	$Far.Panel.Path = $target
	$Far.Panel.Redraw()
	return
}

if ($Edit -or $View) {
	if (![IO.File]::Exists($target)) {
		return Show-FarMsg "Target file does not exist: '$target'"
	}

	if ($Edit) {
		Start-FarEditor $target
	}
	else {
		Start-FarViewer $target
	}

	return
}

if ([IO.File]::Exists($target)) {
	$Far.Panel.GoToPath($target)
	return
}

Show-FarMsg "Target does not exist: '$target'"
