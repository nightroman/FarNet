
<#
.Synopsis
	Shows command, folder or view history.
	Author: Roman Kuzmin

.Description
	It collects and shows Far command, folder or view history in a list menu.
	Selected command is posted to Far (assuming the command line); selected
	folder is opened in the active panel; selected file is opened in the
	editor.

	For quick incremental filter of the list just type a substring. Use
	[AltDown] to set or change a permanent filter.
#>

param
(
	[switch]
	# Show folder history.
	$Folders
	,
	[switch]
	# Show view\edit file history.
	$View
)

function Menu($Title, $Key)
{
	$input | Out-FarList -SelectLast -Title $Title -FilterHistory "Filter$Key" -FilterRestore
}

if ($Folders) {
	### folder history
	$filter = $null
	if ($Far.Window.Kind -eq 'Panels') {
		if (!$Far.Panel.IsPlugin) {
			$filter = '0'
		}
	}
	$Far.GetHistory('SavedFolderHistory', $filter) |
	Menu 'Folders History' 'SavedFolderHistory' | .{process{
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
elseif ($View) {
	### file history
	#! do not make unique, not trivial, we need latest dupes
	$path = $Far.GetHistory('SavedViewHistory', '01') |
	Menu 'Files history' 'SavedViewHistory' |
	Open-FarEditor
}
else {
	### command history
	$Far.GetHistory('SavedHistory') |
	Menu 'History' 'SavedHistory' | .{process{
		$Far.PostText($_, $false)
	}}
}
