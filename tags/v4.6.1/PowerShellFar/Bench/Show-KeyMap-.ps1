
<#
.Synopsis
	Shows Far hotkeys and macros by areas (HTML page).
	Author: Roman Kuzmin

.Description
	Generates and shows an HTML page with Far keys and macros by areas. Area
	table colums are: key name, default Far action, common macro, area macro.
	It is useful to be able to see all keys together.

.Example
	# Shows all keys:
	Show-KeyMap-

.Example
	# Shows specified keys:
	Show-KeyMap- Del, ShiftDel, F8, ShiftF8
#>

param
(
	[string[]]
	# Subset of keys to be shown. Default: empty, all keys are shown.
	$Name
	,
	# Output HTML file path. Default: $HOME\FarKeyMap.output.htm
	$Output = "$HOME\FarKeyMap.output.htm"
)

# HtmlEncode
Add-Type -AssemblyName System.Web

$mapShell = @{ ### SHELL MAP
'Add' = 'Select group'
'AltAdd' = 'Select files with the same name as current'
'AltDel' = 'Wipe'
'AltEnd' = 'Scroll long names and descriptions'
'AltF1' = 'Change the current drive for left panel'
'AltF10' = 'Perform find folder command'
'AltF11' = 'Display view and edit history'
'AltF12' = 'Display folders history'
'AltF2' = 'Change the current drive for right panel'
'AltF3' = 'Internal/external viewer'
'AltF4' = 'Internal/external editor'
'AltF5' = 'Print files'
'AltF6' = 'Create file links (NTFS only)'
'AltF7' = 'Perform find file command'
'AltF8' = 'Display commands history'
'AltHome' = 'Scroll long names and descriptions'
'AltLeft' = 'Scroll long names and descriptions'
'AltRight' = 'Scroll long names and descriptions'
'AltShift[' = 'Insert network (UNC) path from the active panel'
'AltShift]' = 'Insert network (UNC) path from the passive panel'
'AltShiftF9' = 'Configure plugin modules'
'AltShiftIns' = 'Copy full names of selected files to the clipboard'
'AltSubtract' = 'Deselect files with the same name as current'
'BS' = 'Delete char left'
'Clear' = 'View'
'Ctrl;' = 'Insert full file name from the passive panel'
'Ctrl[' = 'Insert path from the left panel'
'Ctrl]' = 'Insert path from the right panel'
'Ctrl0' = 'L|R: Set alternative full view mode | Go to folder shortcut'
'Ctrl1' = 'L|R: Set brief view mode | Go to folder shortcut'
'Ctrl2' = 'L|R: Set medium view mode | Go to folder shortcut'
'Ctrl3' = 'L|R: Set full view mode | Go to folder shortcut'
'Ctrl4' = 'L|R: Set wide view mode | Go to folder shortcut'
'Ctrl5' = 'L|R: Set detailed view mode | Go to folder shortcut'
'Ctrl6' = 'L|R: Set descriptions view mode | Go to folder shortcut'
'Ctrl7' = 'L|R: Set long descriptions view mode | Go to folder shortcut'
'Ctrl8' = 'L|R: Set file owners view mode | Go to folder shortcut'
'Ctrl9' = 'L|R: Set file links view mode | Go to folder shortcut'
'CtrlA' = 'Set file attributes'
'CtrlAdd' = 'Select files with the same extension as current'
'CtrlAlt;' = 'Insert network (UNC) file name from the passive panel'
'CtrlAlt[' = 'Insert network (UNC) path from the left panel'
'CtrlAlt]' = 'Insert network (UNC) path from the right panel'
'CtrlAltClear' = 'Restore default panels height'
'CtrlAltF' = 'Insert network (UNC) file name from the active panel'
'CtrlAltIns' = 'Copy selected network (UNC) names to the clipboard'
'CtrlB' = 'Show/Hide functional key bar at the bottom line'
'CtrlBackSlash' = 'Change to the root folder (Ctrl\)'
'CtrlBS' = 'Delete word left'
'CtrlClear' = 'Restore default panels width'
'CtrlD' = 'Character right'
'CtrlDel' = 'Delete word right'
'CtrlDown' = 'Change panels height'
'CtrlE' = 'Previous command'
'CtrlEnd' = 'End of line'
'CtrlEnter' = 'Insert file name from the active panel'
'CtrlF' = 'Insert full file name from the active panel'
'CtrlF1' = 'Hide/Show left panel'
'CtrlF10' = 'Sort files by description'
'CtrlF11' = 'Sort files by file owner'
'CtrlF12' = 'Display sort modes menu'
'CtrlF2' = 'Hide/Show right panel'
'CtrlF3' = 'Sort files by name'
'CtrlF4' = 'Sort files by extension'
'CtrlF5' = 'Sort files by modification time'
'CtrlF6' = 'Sort files by size'
'CtrlF7' = 'Keep files unsorted'
'CtrlF8' = 'Sort files by creation time'
'CtrlF9' = 'Sort files by access time'
'CtrlG' = 'Apply command to selected files'
'CtrlH' = 'Toggle hidden and system files displaying'
'CtrlHome' = 'Start of line'
'CtrlIns' = 'Copy the names of selected files to the clipboard'
'CtrlJ' = 'Insert current file name from the active panel'
'CtrlK' = 'Delete to end of line'
'CtrlL' = 'Toggle info panel'
'CtrlLeft' = 'Change panels width | Word left'
'CtrlM' = 'Restore previous selection'
'CtrlMultiply' = 'Invert selection including folders'
'CtrlN' = 'Toggle long/short file names view mode'
'CtrlNum2' = 'Change panels height'
'CtrlNum4' = 'Change panels width'
'CtrlNum6' = 'Change panels width'
'CtrlNum8' = 'Change panels height'
'CtrlO' = 'Hide/show both panels'
'CtrlP' = 'Hide/show inactive panel'
'CtrlPgDn' = 'Change folder, enter an archive (also a SFX archive)'
'CtrlPgUp' = 'Change to the parent folder'
'CtrlQ' = 'Toggle quick view panel'
'CtrlR' = 'Re-read panel'
'CtrlRight' = 'Change panels width | Word right'
'CtrlS' = 'Character left'
'CtrlShift[' = 'Insert path from the active panel'
'CtrlShift]' = 'Insert path from the passive panel'
'CtrlShift0' = 'Create shortcut to the current folder'
'CtrlShift1' = 'Create shortcut to the current folder'
'CtrlShift2' = 'Create shortcut to the current folder'
'CtrlShift3' = 'Create shortcut to the current folder'
'CtrlShift4' = 'Create shortcut to the current folder'
'CtrlShift5' = 'Create shortcut to the current folder'
'CtrlShift6' = 'Create shortcut to the current folder'
'CtrlShift7' = 'Create shortcut to the current folder'
'CtrlShift8' = 'Create shortcut to the current folder'
'CtrlShift9' = 'Create shortcut to the current folder'
'CtrlShiftDown' = 'Change current panel height'
'CtrlShiftEnter' = 'Insert current file name from the passive panel'
'CtrlShiftF3' = 'View'
'CtrlShiftF4' = 'Edit'
'CtrlShiftIns' = 'Copy the names of selected files to the clipboard'
'CtrlShiftNum2' = 'Change current panel height'
'CtrlShiftNum8' = 'Change current panel height'
'CtrlShiftUp' = 'Change current panel height'
'CtrlSubtract' = 'Deselect files with the same extension as current'
'CtrlT' = 'Toggle tree panel'
'CtrlU' = 'Swap panels'
'CtrlUp' = 'Change panels height'
'CtrlX' = 'Next command'
'CtrlY' = 'Clear command line'
'CtrlZ' = 'Describe selected files'
'Del' = 'Delete char'
'Enter' = 'Execute, change folder, enter to an archive'
'F1' = 'Online help'
'F10' = 'Quit Far'
'F11' = 'Show the plugin commands menu'
'F2' = 'Show user menu'
'F3' = 'View'
'F4' = 'Edit'
'F5' = 'Copy'
'F6' = 'Rename or move'
'F7' = 'Create new folder'
'F8' = 'Delete'
'F9' = 'Show menus bar'
'Ins' = 'Select/deselect file'
'Left' = 'Character left'
'Multiply' = 'Invert selection'
'Right' = 'Character right'
'ShiftAdd' = 'Select all files'
'ShiftDel' = 'Delete'
'ShiftDown' = 'Select/deselect file'
'ShiftEnd' = 'Select/deselect file'
'ShiftEnter' = 'Execute in the separate window'
'ShiftF1' = 'Add files to archive'
'ShiftF10' = 'Select last executed menu item'
'ShiftF11' = 'Use group sorting'
'ShiftF12' = 'Show selected files first'
'ShiftF2' = 'Extract files from archive'
'ShiftF3' = 'Perform archive managing commands'
'ShiftF4' = 'Edit new file'
'ShiftF5' = 'Copy file under cursor'
'ShiftF6' = 'Rename or move file under cursor'
'ShiftF8' = 'Delete file under cursor'
'ShiftF9' = 'Save configuration'
'ShiftHome' = 'Select/deselect file'
'ShiftIns' = 'Paste from clipboard'
'ShiftLeft' = 'Select/deselect file'
'ShiftPgDn' = 'Select/deselect file'
'ShiftPgUp' = 'Select/deselect file'
'ShiftRight' = 'Select/deselect file'
'ShiftSubtract' = 'Deselect all files'
'ShiftUp' = 'Select/deselect file'
'Subtract' = 'Deselect group'
'Tab' = 'Change active panel'
}

$mapEditor = @{ ### EDITOR MAP
'AltBS' = 'Undo'
'AltDown' = 'Select vertical block'
'AltF11' = 'Display edit history'
'AltF5' = 'Print file or selected block ("Print manager" plugin is used)'
'AltF8' = 'Go to specified line and column'
'AltI' = 'Shift block right'
'AltLeft' = 'Select vertical block'
'AltRight' = 'Select vertical block'
'AltShiftDown' = 'Select vertical block'
'AltShiftEnd' = 'Select vertical block'
'AltShiftF9' = 'Call editor settings dialog'
'AltShiftHome' = 'Select vertical block'
'AltShiftLeft' = 'Select vertical block'
'AltShiftPgDn' = 'Select vertical block'
'AltShiftPgUp' = 'Select vertical block'
'AltShiftRight' = 'Select vertical block'
'AltShiftUp' = 'Select vertical block'
'AltU' = 'Shift block left'
'AltUp' = 'Select vertical block'
'BS' = 'Delete char left'
'Ctrl0' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl1' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl2' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl3' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl4' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl5' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl6' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl7' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl8' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl9' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'CtrlA' = 'Select all text'
'CtrlAdd' = 'Append block to clipboard'
'CtrlB' = 'Show/Hide functional key bar at the bottom line'
'CtrlBS' = 'Delete word left'
'CtrlC' = 'Copy block to clipboard'
'CtrlD' = 'Delete block'
'CtrlDel' = 'Delete word right'
'CtrlDown' = 'Scroll screen down'
'CtrlE' = 'End of screen'
'CtrlEnd' = 'End of file'
'CtrlF' = 'Insert full name of the file being edited at the cursor position.'
'CtrlF10' = 'Position to the current file'
'CtrlF7' = 'Replace'
'CtrlHome' = 'Start of file'
'CtrlIns' = 'Copy block to clipboard'
'CtrlK' = 'Delete to end of line'
'CtrlL' = 'Disable edited text modification'
'CtrlLeft' = 'Word left'
'CtrlM' = 'Move block to current cursor position (in persistent blocks mode only)'
'CtrlN' = 'Start of screen'
'CtrlO' = 'Show user screen'
'CtrlP' = 'Copy block to current cursor position (in persistent blocks mode only)'
'CtrlPgDn' = 'End of file'
'CtrlPgUp' = 'Start of file'
'CtrlQ' = 'Treat the next key combination as a character code'
'CtrlRight' = 'Word right'
'CtrlS' = "Move cursor one char left (but not to the previous line)"
'CtrlShift0' = 'Set a bookmark at the current position'
'CtrlShift1' = 'Set a bookmark at the current position'
'CtrlShift2' = 'Set a bookmark at the current position'
'CtrlShift3' = 'Set a bookmark at the current position'
'CtrlShift4' = 'Set a bookmark at the current position'
'CtrlShift5' = 'Set a bookmark at the current position'
'CtrlShift6' = 'Set a bookmark at the current position'
'CtrlShift7' = 'Set a bookmark at the current position'
'CtrlShift8' = 'Set a bookmark at the current position'
'CtrlShift9' = 'Set a bookmark at the current position'
'CtrlShiftB' = 'Show/Hide status line'
'CtrlShiftDown' = 'Select block'
'CtrlShiftEnd' = 'Select block'
'CtrlShiftEnter' = 'Insert file name from the passive panel'
'CtrlShiftHome' = 'Select block'
'CtrlShiftLeft' = 'Select block'
'CtrlShiftPgDn' = 'Select block'
'CtrlShiftPgUp' = 'Select block'
'CtrlShiftRight' = 'Select block'
'CtrlShiftUp' = 'Select block'
'CtrlShiftZ' = 'Redo'
'CtrlT' = 'Delete word right'
'CtrlU' = 'Deselect block'
'CtrlUp' = 'Scroll screen up'
'CtrlV' = 'Paste block from clipboard'
'CtrlX' = 'Cut block'
'CtrlY' = 'Delete line'
'CtrlZ' = 'Undo'
'Del' = 'Delete char (also may delete block, depending on editor settings)'
'Down' = 'Line down'
'End' = 'End of line'
'Esc' = 'Quit'
'F1' = 'Help'
'F10' = 'Quit'
'F11' = 'Show the plugin commands menu'
'F2' = 'Save file'
'F4' = 'Quit'
'F6' = 'Switch to viewer'
'F7' = 'Search'
'F8' = 'Toggle DOS/Windows text'
'Home' = 'Start of line'
'Left' = 'Character left'
'PgDn' = 'Page down'
'PgUp' = 'Page up'
'Right' = 'Character right'
'ShiftDel' = 'Cut block'
'ShiftDown' = 'Select block'
'ShiftEnd' = 'Select block'
'ShiftEnter' = 'Insert file name from the active panel'
'ShiftF10' = 'Save and quit'
'ShiftF2' = 'Save file as...'
'ShiftF4' = 'Edit new file'
'ShiftF7' = 'Continue search/replace'
'ShiftF8' = 'Select custom character table'
'ShiftHome' = 'Select block'
'ShiftIns' = 'Paste block from clipboard'
'ShiftLeft' = 'Select block'
'ShiftPgDn' = 'Select block'
'ShiftPgUp' = 'Select block'
'ShiftRight' = 'Select block'
'ShiftUp' = 'Select block'
'Up' = 'Line up'
}

$mapViewer = @{ ### VIEWER MAP
'Add' = 'Go to next file'
'AltBS' = 'Undo position change'
'AltF11' = 'Display view history'
'AltF5' = 'Print the file ("Print manager" plugin is used)'
'AltF7' = 'Continue search in "reverse" mode'
'AltF8' = 'Change current position'
'AltShiftF9' = 'Call viewer settings'
'Clear' = 'Quit'
'Ctrl0' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl1' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl2' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl3' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl4' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl5' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl6' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl7' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl8' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'Ctrl9' = 'L|R: Go to bookmark | Set a bookmark at the current position'
'CtrlB' = 'Show/Hide functional key bar at the bottom line'
'CtrlC' = 'Copy the text highlighted as a result of the search to the clipboard'
'CtrlEnd' = 'End of file'
'CtrlF10' = 'Position to the current file.'
'CtrlHome' = 'Start of file'
'CtrlIns' = 'Copy the text highlighted as a result of the search to the clipboard'
'CtrlLeft' = '20 characters left. In Hex-mode - 1 position left'
'CtrlO' = 'Show user screen'
'CtrlRight' = '20 characters right. In Hex-mode - 1 position right'
'CtrlS' = 'Show/Hide the scrollbar'
'CtrlShift0' = 'Set a bookmark at the current position'
'CtrlShift1' = 'Set a bookmark at the current position'
'CtrlShift2' = 'Set a bookmark at the current position'
'CtrlShift3' = 'Set a bookmark at the current position'
'CtrlShift4' = 'Set a bookmark at the current position'
'CtrlShift5' = 'Set a bookmark at the current position'
'CtrlShift6' = 'Set a bookmark at the current position'
'CtrlShift7' = 'Set a bookmark at the current position'
'CtrlShift8' = 'Set a bookmark at the current position'
'CtrlShift9' = 'Set a bookmark at the current position'
'CtrlShiftB' = 'Show/Hide status line'
'CtrlShiftLeft' = 'Start of lines on the screen'
'CtrlShiftRight' = 'End of lines on the screen'
'CtrlU' = 'Remove the highlighting of the search results'
'CtrlZ' = 'Undo position change'
'Down' = 'Line down'
'End' = 'End of file'
'Esc' = 'Quit'
'F1' = 'Help'
'F10' = 'Quit'
'F11' = 'Show the plugin commands menu'
'F2' = 'Toggle line wrap/unwrap'
'F3' = 'Quit'
'F4' = 'Toggle text/hex mode'
'F6' = 'Switch to editor'
'F7' = 'Search'
'F8' = 'Toggle DOS/Windows text view mode'
'Home' = 'Start of file'
'Left' = 'Character left'
'PgDn' = 'Page down'
'PgUp' = 'Page up'
'Right' = 'Character right'
'ShiftF2' = 'Toggle wrap type (letters/words)'
'ShiftF7' = 'Continue search'
'ShiftF8' = 'Select custom character table'
'Space' = 'Continue search'
'Subtract' = 'Go to previous file'
'Up' = 'Line up'
}

$mapDialog = @{ ### DIALOG MAP
'CtrlDown' = 'Open combobox and history lists'
'CtrlEnter' = 'Close dialog'
'CtrlF5' = 'Move dialog'
'Down' = 'Navigation'
'Enter' = 'Close dialog'
'Esc' = 'Cancel dialog'
'F1' = 'Help'
'F11' = 'Show the plugin commands menu'
'Left' = 'Navigation'
'Right' = 'Navigation'
'ShiftTab' = 'Navigation'
'Space' = 'Select button'
'Tab' = 'Navigation'
'Up' = 'Navigation'
}

function GetMacroMap($Area)
{
	$map = @{}
	foreach($name in $Far.Macro.GetNames($Area)) {
		$data = $Far.Macro.GetMacro($Area, $name)
		$desc = $data.Description
		if (!$desc.Trim()) {
			$desc = '({0})' -f $data.Sequence
		}
		if ($data.IsRestricted()) {
			$desc = '+ ' + $desc
		}
		$map[$name] = $desc
	}
	$map
}

function OutAreaTable($Area, $Default, $Common, $Macro)
{
	# table:
	@'
<h2><a name="#{0}">{0} Key Map</a></h2>
<table width="100%">
<tr>
<th>Key Name</th>
<th>Default Action</th>
<th>Common Macro</th>
<th>{0} Macro</th>
</tr>
'@ -f $Area

	foreach($key in .{ $Default.Keys; $Common.Keys; $Macro.Keys } | Sort-Object -Unique) {
		if (!$Name -or $Name -contains $key) {

			# check the key spelling (round-trip)
			$code = $Far.NameToKey($key)
			if ($code -lt 0) { throw "Invalid key name: '$key'" }
			if ($Far.KeyToName($code) -cne $key) { throw "Not standard key name: replace '$key' with '$($Far.KeyToName($code))'" }

			# row, data: key name
			"<tr><td><code>$Area $key</code></td>"

			$defaultAction = $Default[$key]
			$commonMacro = $Common[$key]
			$areaMacro = $Macro[$key]

			# data: default action
			'<td>'
			if ($defaultAction) {
				$defaultAction = [System.Web.HttpUtility]::HtmlEncode($defaultAction)
				if ($commonMacro -or $areaMacro) {
					"<strike>$defaultAction</strike>"
				}
				else {
					$defaultAction
				}
			}
			'</td>'

			# data: common macro
			'<td>'
			if ($commonMacro) {
				$commonMacro = [System.Web.HttpUtility]::HtmlEncode($commonMacro)
				if ($areaMacro) {
					"<strike>$commonMacro</strike>"
				}
				else {
					$commonMacro
				}
			}
			'</td>'

			# data: area macro
			'<td>'
			if ($areaMacro) {
				[System.Web.HttpUtility]::HtmlEncode($areaMacro)
			}
			'</td>'

			'</tr>'
		}
	}
	'</table>'
}

### Output
.{
	@'
<html>
<head>
<title>Far Manager Key Map</title>
<style>
th { padding: 4px; background-color: silver }
td { padding: 4px; background-color: #eeeeee }
</style>
</head>
<body>
<h1>Far Manager Key Map</h1>
<ul>
<li><a href="#Shell">Shell</a></li>
<li><a href="#Editor">Editor</a></li>
<li><a href="#Viewer">Viewer</a></li>
<li><a href="#Dialog">Dialog</a></li>
</ul>
'@

	### Special keys
	@'
<hr/>
<h2>Special Keys</h2>
<table>
<tr>
<th>Key Name</th>
<th>Action or Meaning</th>
</tr>

<tr><td><code>AltF9</code></td><td>Toggle the console window size</td></tr>
<tr><td><code>AltIns</code></td><td>Start screen block selection</td></tr>

<tr><td><code>CtrlAltShift</code></td><td>Temporarily hide the current window</td></tr>
<tr><td><code>CtrlShiftTab</code></td><td>Switch to the previous window</td></tr>
<tr><td><code>CtrlTab</code></td><td>Switch to the next window</td></tr>
<tr><td><code>CtrlW</code></td><td>Show the task list</td></tr>

<tr><td><code>*Add</code></td><td>Gray +</td></tr>
<tr><td><code>*Clear</code></td><td>Numpad 5</td></tr>
<tr><td><code>*Multiply</code></td><td>Gray *</td></tr>
<tr><td><code>*Subtract</code></td><td>Gray -</td></tr>

<tr><td><code>*MsLClick</code></td><td>Mouse left click</td></tr>
<tr><td><code>*MsRClick</code></td><td>Mouse right click</td></tr>
<tr><td><code>*MsM1Click</code></td><td>Mouse middle click</td></tr>
<tr><td><code>*MsWheelUp</code></td><td>Mouse wheels up</td></tr>
<tr><td><code>*MsWheelDown</code></td><td>Mouse wheels down</td></tr>

</table>
<hr/>
'@

	# out a table for each area with a column for the common area
	$macroCommon = GetMacroMap 'Common'
	OutAreaTable 'Shell' $mapShell $macroCommon (GetMacroMap 'Shell')
	OutAreaTable 'Editor' $mapEditor $macroCommon (GetMacroMap 'Editor')
	OutAreaTable 'Viewer' $mapViewer $macroCommon  (GetMacroMap 'Viewer')
	OutAreaTable 'Dialog' $mapDialog $macroCommon  (GetMacroMap 'Dialog')

@"
<hr/>
<p>
$([DateTime]::Now) Show-KeyMap-.ps1
</p>
</body>
</html>
"@
} > $Output

Invoke-Item $Output
