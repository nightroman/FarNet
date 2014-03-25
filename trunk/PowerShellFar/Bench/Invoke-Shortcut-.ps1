
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

	[F9] \ Commands \ File associations

		Mask: *.lnk
		[Enter]
			ps: Invoke-Shortcut-.ps1 #
		[CtrlPgDn]
			ps: Invoke-Shortcut-.ps1 -Panel #
		[F3]
			ps: Invoke-Shortcut-.ps1 -View #
		[F4]
			ps: Invoke-Shortcut-.ps1 -Edit #

	[Shift-Enter] still opens shortcuts as Windows.

.Parameter Path
		Path of the .lnk file to be opened. Default: the current panel item.
.Parameter Panel
		Tells to show shortcut properties in a panel.
.Parameter Edit
		Tells to edit the target file in the editor.
.Parameter View
		Tells to view the target file in the viewer.
#>

param(
	[Parameter(Mandatory=1)]
	[string]$Path = (Get-FarPath),
	[switch]$Panel,
	[switch]$Edit,
	[switch]$View
)

$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)
Assert-Far ([System.IO.File]::Exists($Path)) "Missing file '$Path'." Invoke-Shortcut-.ps1

# WMI does not work well with names with spaces, use WScript.Shell.
# Besides, WScript.Shell also works with .url, just in case
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut([IO.Path]::GetFullPath($Path))
$target = $link.TargetPath
Assert-Far ([bool]$target) "Cannot get a target path from '$Path'.`nIs it a shortcut file?" Invoke-Shortcut-.ps1

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
if ([System.IO.Directory]::Exists($target)) {
	$Far.Panel.CurrentDirectory = $target
	$Far.Panel.Redraw()
}

### Test a file
elseif (![System.IO.File]::Exists($target)) {
	Show-FarMessage "Missing target '$target'."
}

### Edit a file
elseif ($Edit) {
	Open-FarEditor $target
}

### View a file
elseif ($View) {
	Open-FarViewer $target
}

### Go to
else {
	$Far.Panel.GoToPath($target)
}
