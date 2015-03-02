
<#
.Synopsis
	Shows command, folder, or editor history.
	Author: Roman Kuzmin

.Description
	The script shows the command, folder, or editor history in a menu.

	A selected command is inserted to the current editor line.
	A selected folder is set current in the active panel.
	A selected file is opened in the editor.

	For incremental filtering of the list just type a substring.

.Parameter Folder
		Tells to show folder history.

.Parameter Editor
		Tells to show editor history.

.Link
	PowerShellFar.macro.lua
#>

param(
	[switch]$Folder,
	[switch]$Editor
)

function Select-History($Title) {
	$input | .{process{ $_.Name }} | Out-FarList -SelectLast -Title $Title
}

### folder history
if ($Folder) {
	if (!($r = $Far.History.Folder() | Select-History 'Folder history')) {return}
	try {
		$Far.Panel.CurrentDirectory = $r
	}
	catch {
		Show-FarMessage "Cannot set directory '$r'." 'Folder history'
	}
}
### editor history
elseif ($Editor) {
	$Far.History.Editor() | Select-History 'Editor history' | Open-FarEditor
}
### command history
else {
	if (!($r = $Far.History.Command() | Select-History 'Command history')) {return}
	if ($line = $Far.Line) {
		$line.InsertText($r)
	}
}
