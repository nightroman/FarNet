
<#
.Synopsis
	Opens a shortcut file (.lnk) in a few ways.
	Author: Roman Kuzmin

.Description
	The script gets a target path from the shortcut file (.lnk). If it is a
	directory path this directory is opened on the current panel. If it is a
	file path the action depends on parameters; default action is to jump to
	the target file, other actions: edit or view the file.

	Switch -Panel opens a shortcut in a panel to view and edit its properties.
	If you change data it asks you to save changes on exit or you can use
	[CtrlS] to save data at any moment.

	How to open .lnk files, e.g. by [Enter], [CtrlPgDn], [F3], [F4]:
	Open menu Commands \ File Associations, add an association and set:
	-- Mask: *.lnk
	-- Command for [Enter]: >: Invoke-Shortcut- #
	-- Command for [CtrlPgDn]: >: Invoke-Shortcut- -Panel #
	-- Command for [F3]: >: Invoke-Shortcut- -View #
	-- Command for [F4]: >: Invoke-Shortcut- -Edit #

	[Shift-Enter] still opens shortcut files from Far in standard Windows way.
#>

[CmdletBinding()]
param
(
	# Path of the .lnk file to be opened. Default: the current panel item.
	$Path = (Get-FarPath)
	,
	[switch]
	# To show shortcut properties in a panel.
	$Panel
	,
	[switch]
	# To edit the target file in the Far editor.
	$Edit
	,
	[switch]
	# To view the target file in the Far viewer.
	$View
)

Assert-Far ([IO.File]::Exists($Path)) "File does not exist: '$Path'" "Invoke-Shortcut-.ps1"

# WMI does not work well with names with spaces, use WScript.Shell.
# Besides, WScript.Shell also works with .url, just in case
$WshShell = New-Object -ComObject WScript.Shell
$link = $WshShell.CreateShortcut([IO.Path]::GetFullPath($Path))
$target = $link.TargetPath
Assert-Far ([bool]$target) "Cannot get a target path from '$Path'.`nIs it a shortcut file?" "Invoke-Shortcut-.ps1"

### Panel properties
if ($Panel) {
	$p = New-Object PowerShellFar.MemberPanel $link -Property @{
		Title = "Shortcut $Path"
		AsSaveData = {
			$this.Value.Save()
			$this.Modified = $false
		}
	}
	$p.Open()
	return
}

### Go to directory
if ([IO.Directory]::Exists($target)) {
	$Far.Panel.CurrentDirectory = $target
	$Far.Panel.Redraw()
}

### Test a file
elseif (![IO.File]::Exists($target)) {
	Show-FarMessage "Target file does not exist: '$target'"
}

### Edit a file
elseif ($Edit) {
	Open-FarEditor $target
}

### View a file
elseif ($View) {
	Open-FarViewer $target
}

### Goto a file
else {
	$Far.Panel.GoToPath($target)
}
