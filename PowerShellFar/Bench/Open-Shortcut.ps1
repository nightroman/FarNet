<#
.Synopsis
	Opens a shortcut file (.lnk).
	Author: Roman Kuzmin

.Description
	The script gets a target path from the shortcut file (.lnk). If it is a
	directory path this directory is opened on the current panel. If it is a
	file path the action depends on parameters; default action is to jump to
	the target file, other actions: edit or view the file.

	Switch -Panel opens a shortcut in a panel to view and edit its properties.
	If you change data it asks you to save changes on exit or you can use
	[CtrlS] to save data at any moment.

	HOW TO OPEN .LNK FILES FROM PANELS

	Commands / File associations

		Mask: *.lnk
		[Enter]
			@vps:Open-Shortcut.ps1
		[CtrlPgDn]
			@ps:Open-Shortcut.ps1 -Panel
		[F3]
			@ps:Open-Shortcut.ps1 -View
		[F4]
			@ps:Open-Shortcut.ps1 -Edit

	[Shift-Enter] still opens shortcuts as Windows.

.Parameter Path
		Path of the .lnk file to be opened.
		Default: the cursor panel item.

.Parameter Panel
		Tells to show shortcut properties in a panel.

.Parameter Edit
		Tells to edit the target file in the editor.

.Parameter View
		Tells to view the target file in the viewer.
#>

[CmdletBinding()]
param(
	[string]$Path
	,
	[switch]$Panel
	,
	[switch]$Edit
	,
	[switch]$View
)

#requires -Version 7.4
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$Path = Resolve-Path -LiteralPath ($Path ? $Path : (Get-FarPath))

# WMI does not work well with names with spaces, use WScript.Shell.
# Besides, WScript.Shell also works with .url, just in case
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($Path)
$target = $link.TargetPath
if (!$target) {
	throw "Cannot get a target path from '$Path'. Is it a shortcut file?"
}

### Panel properties
if ($Panel) {
	$r = [PowerShellFar.MemberPanel]::new($link)
	$r.Title = "Shortcut $Path"
	$r.AsSaveData = {
		$this.Value.Save()
		$this.Modified = $false
	}
	return $r.Open()
}

### Go to directory
if ([System.IO.Directory]::Exists($target)) {
	$Far.Panel.CurrentDirectory = $target
	$Far.Panel.Redraw()
}

### Test a file
elseif (![System.IO.File]::Exists($target)) {
	Show-FarMessage "Missing target '$target'."
}

### View a file
elseif ($View -or ($Edit -and ($target -like '*.exe'))) {
	Open-FarViewer $target
}

### Edit a file
elseif ($Edit) {
	Open-FarEditor $target
}

### Go to
else {
	$Far.Panel.GoToPath($target)
}
