
<#
.SYNOPSIS
	Shows command, folder or view history
	Author: Roman Kuzmin

.DESCRIPTION
	It collects and shows Far command, folder or view history in a list menu.
	Selected command is posted to Far (assuming the command line); selected
	folder is opened in the active panel; selected file is opened in the
	editor.

	For quick incremental filter of the list just type a substring. Use
	[AltDown] to set or change a permanent filter.

.PARAMETER Folders
		Show folder history.
.PARAMETER View
		Show view\edit file history.
#>

param
(
	[switch]$Folders,
	[switch]$View
)

function Menu($Title, $Key)
{
	$input | Out-FarList -SelectLast -Title $Title -FilterHistory "Filter$Key" -FilterRestore
}

if ($Folders) {
	### folder history
	$filter = $null
	if ($Far.WindowType -eq 'Panels') {
		if (!$Far.Panel.IsPlugin) {
			$filter = '0'
		}
	}
	$Far.GetHistory('SavedFolderHistory', $filter) |
	Menu 'Folders History' 'SavedFolderHistory' | .{process{
		if ($_) {
			# _090929_061740
			if (($_.Length -lt 260) -and !([System.IO.Directory]::Exists($_))) {
				Show-FarMsg "Directory '$_' does not exist."
			}
			else {
				$Far.Panel.Path = $_
				if ($Far.WindowType -eq 'Dialog') {
					$Far.Redraw()
				}
			}
		}
	}}
}
elseif ($View) {
	### file history
	#! do not make unique, not trivial, we need latest dupes
	$path = $Far.GetHistory('SavedViewHistory', '01') |
	Menu 'Files history' 'SavedViewHistory' |
	Start-FarEditor
}
else {
	### command history
	$Far.GetHistory('SavedHistory') |
	Menu 'History' 'SavedHistory' | .{process{
		$Far.PostText($_, $false)
	}}
}
