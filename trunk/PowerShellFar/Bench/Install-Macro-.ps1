
<#
.SYNOPSIS
	Installs macros for some PowerShellFar tools (template).
	Author: Roman Kuzmin

.DESCRIPTION
	This script installs key macros for tools mostly called from PowerShellFar
	menu, see Profile-.ps1 for its example. This macro set is used by the
	author and provided only for demo, you should use your own.

	These macros covers only a part of available PowerShellFar tools. But they
	should give an idea how to assign macros to other user tools.

	For example in editor: "Go\Select to extended home" (see Go-Home-.ps1 and
	menu) are not covered by macros, because the author uses event handler way
	for [Home]\[ShiftHome], see Profile-Editor-.ps1. It is difficult to say
	what way is better for keys: macros or events.

.LINK
	FarNet IKeyMacroHost.Install
#>

param
(
	# PowerShellFar hotkey in the plugin commands menu.
	$P = '1'
)

# Note: it is recommended to install all macros by one call of Install(). This
# protects you from unintended installation of the same macro twice by mistake.
$Far.KeyMacro.Install(@(

	### Common (all areas)
	@{ Area = 'Common' }

	### PowerShellFar command history
	# Also, AltF10 in panels disables questionable folder tree feature.
	@{ Name = 'AltF10'; Description = 'PSF: Command history'; Sequence = "F11 $P 4" }

	### Open recent file in editor
	# CtrlShiftF11 -> standard AltF11
	@{ Name = 'CtrlShiftF11'; Description = 'Far: Open recent file'; Sequence = 'AltF11' }
	# Calls Show-History-.ps1 or fallback
	@{
		Name = 'AltF11'
		Description = 'PSF: Edit recent file'
		Sequence = 'F11 $if(Menu.Select("PowerShellFar", 2) > 0) Enter - e $else Esc AltF11 $end'
	}

	$null

	### Mixed (several areas)

	### (Edit-FarDescription-.ps1) Edit file description: Shell: current item; Editor or Viewer: opened file
	@{ Name = 'AltZ'; Sequence = "F11 $P t"; Description = 'PSF: Edit description' }
	@{ Area = 'Shell' }, @{ Area = 'Editor' }, @{ Area = 'Viewer' }
	$null

	### (Complete-Word-.ps1) Command line: from history; Editor: from file; Dialog: from edit box history
	@{ Name = 'CtrlSpace'; Sequence = "F11 $P c"; Description = 'PSF: Complete word' }
	@{ Area = 'Shell' }, @{ Area = 'Editor' }, @{ Area = 'Dialog' }
	$null

	### (Set-Selection-.ps1) Change selected text case to lower\upper in command line, editor or dialog edit box
	@{ Description = 'PSF: Selection to lower\upper'; SelectedText = '1' }
	@{ Name = 'CtrlU'; Sequence = "F11 $P l"; Area = 'Shell' }, @{ Area = 'Editor' }, @{ Area = 'Dialog' }
	@{ Name = 'CtrlShiftU'; Sequence = "F11 $P u"; Area = 'Shell' }, @{ Area = 'Editor' }, @{ Area = 'Dialog' }
	$null

	### Shell only

	### Easy prefix: space expands empty command line to '>: '
	@{ Area = 'Shell'; Name = 'Space'; Sequence = "> : Space"; CommandLine = '0'; Description = 'PSF: Easy prefix' }
	$null

	### Easy invoke: type and run without prefix (Invoke selected code)
	@{ Area = 'Shell'; Name = 'ShiftSpace'; Sequence = "F11 $P 2"; CommandLine = '1'; Description = 'PSF: Easy invoke' }
	$null

	### Other Shell macros
	@{ Area = 'Shell' }
	### (Go-Head-.ps1) Go to head file item (e.g. useful after [CtrlF5] to find the newest file)
	@{ Name = 'CtrlShiftF5'; Sequence = "F11 $P h"; Description = 'PSF: Go to panel head item' }

	### Open recent folder in the panel
	### CtrlShiftF12 -> standard AltF12
	@{ Name = 'CtrlShiftF12'; Sequence = "AltF12"; Description = 'Far: Open recent folder' }
	### Call Show-History-.ps1 by PSF or fallback
	@{
		Name = 'AltF12'
		Description = 'PSF: Open recent folder'
		Sequence = 'F11 $if(Menu.Select("PowerShellFar", 2) > 0) Enter - n $else Esc AltF12 $end'
	}

	### (Search-Regex-.ps1) Backgroung search in files with dynamic results in the panel
	@{ Name = 'CtrlShiftF7'; Sequence = "F11 $P x"; Description = 'PSF: Search regex in files' }
	$null

	### Editor only

	### (Indent-Selection-.ps1) Indent and outdent selected line(s)
	@{ Area = 'Editor'; Description = 'PSF: Indent selection'; SelectedText = '1' }
	@{ Name = 'Tab'; Sequence = "F11 $P i" }
	@{ Name = 'ShiftTab'; Sequence = "F11 $P o" }
	$null

	### Other Editor macros
	@{ Area = 'Editor' }
	### (Reindent-Selection-.ps1) Reindent selected lines or the current line
	@{ Name = 'AltF8'; Sequence = "F11 $P r"; Description = 'PSF: Reindent selection' }
	### (Reformat-Selection-.ps1) Reformat selected lines or the current line
	@{ Name = 'CtrlShiftL'; Sequence = "F11 $P f"; Description = 'PSF: Reformat selection' }
	$null

	### Native macros

	### Go to text selection edge in command line, edit boxes, editor
	@{ Description = 'Go to selection edge'; SelectedText = '1' }
	@{ Name = 'Left'; Sequence = "Editor.Sel(1,0) Editor.Sel(4)"; Area = 'Shell' }, @{ Area = 'Dialog' }, @{ Area = 'Editor' }
	@{ Name = 'Right'; Sequence = "Editor.Sel(1,1) Editor.Sel(4)"; Area = 'Shell' }, @{ Area = 'Dialog' }, @{ Area = 'Editor' }
	$null
))
