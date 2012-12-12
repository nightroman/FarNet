
<#
.Synopsis
	Shows command, folder, or editor history.
	Author: Roman Kuzmin

.Description
	It collects and shows command, folder, or editor history in a list menu.
	Selected command is posted to Far (assuming the command line); selected
	folder is opened in the active panel; selected file is opened in the
	editor.

	For incremental filtering of the list just type a substring.

.Parameter Folder
		Show folder history.

.Parameter Editor
		Show editor history.

.Link
	PowerShellFar.macro.lua
#>

param
(
	[switch]$Folder,
	[switch]$Editor
)

function Menu($Title)
{
	$input | .{process{ $_.Name }} | Out-FarList -SelectLast -Title $Title
}

if ($Folder) {
	### folder history
	$Far.History.Folder() |
	Menu 'Folder history' | .{process{
		if ($_) {
			# _090929_061740
			if (($_.Length -lt 260) -and !([System.IO.Directory]::Exists($_))) {
				Show-FarMessage "Directory '$_' does not exist."
			}
			else {
				$Far.Panel.CurrentDirectory = $_
				if ($Far.Window.Kind -eq 'Dialog') {
					$Far.UI.Redraw()
				}
			}
		}
	}}
}
elseif ($Editor) {
	### editor history
	$Far.History.Editor() |
	Menu 'Editor history' |
	Open-FarEditor
}
else {
	### command history
	$Far.History.Command() |
	Menu 'Command history' | .{process{
		$line = $Far.Line
		if ($line) {
			$line.InsertText($_)
		}
	}}
}
