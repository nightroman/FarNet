
<#
.SYNOPSIS
	Installs macros (example).
	Author: Roman Kuzmin

.DESCRIPTION
	DO NOT USE THIS DIRECTLY, USE YOUR OWN MACROS, THIS IS ONLY AN EXAMPLE!

	The script shows how to assign macros to the user tools via menus or the
	PowerShellFar input code box.

	For the editor there is another way: event handlers installed in the editor
	startup code; example: Profile-Editor-.ps1.

.NOTES
	-- It is recommended to install all macros by one call of Install() to make
	sure that there are no duplicates with same area and name.

	-- Note that parameter of Install() in this example is not just an array of
	[FarNet.Macro], it is a block of code which outputs [FarNet.Macro] objects.

.LINK
	FarNet.IMacro.Install
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param()

Import-Module FarMacro

# confirm
if (!$pscmdlet.ShouldProcess($null)) { return }

# Gets a PSF menu macro sequence based on -If assuming the menu item is "...PowerShellFar".
# If PSF is not installed (Far is loaded with /p) the macro invokes the -Else part, if any.
# The -Else part works in the area where the macro starts, not in the plugins menu.
function Get-PsfMacro($If, $Else)
{
	if ($Else) { $Else = 'Esc ' + $Else } else { $Else = 'Esc' }
	'F11 $If (Menu.Select("PowerShellFar", 3) > 0) Enter {0} $Else {1} $End' -f $If, $Else
}

# Gets a sequence for the command line via a macro in the Common area,
# the sequence at first closes the AutoCompletion list if it is shown.
function Get-CommandLineMacro($Sequence)
{
	'$If (AutoCompletion) Esc $End $If (Shell||Info||QView||Tree) {0} $End' -f $Sequence
}

# install
$Far.Macro.Install($(

	### == Common (all areas)

	### Decrease/Increase font size
	New-FarMacro Common CtrlShiftD 'F11 $If (Menu.Select(".NET", 2) > 0) Enter c d $End' 'FarNet: Decrease font size'
	New-FarMacro Common CtrlShiftI 'F11 $If (Menu.Select(".NET", 2) > 0) Enter c i $End' 'FarNet: Increase font size'

	### Favorites menu
	New-FarMacro Common CtrlShiftL (Get-PsfMacro '1 "Menu-Favorites-.ps1" Enter') 'PSF: Favorites'

	### PowerShellFar command history
	# Also, AltF10 in panels disables questionable folder tree feature.
	New-FarMacro Common AltF10 (Get-PsfMacro 4) 'PSF: Command history'

	### Open recent file in editor (history)
	<# 2010-12-08 FarNet.Vessel will do this
	# Calls Show-History-.ps1 or fallback
	New-FarMacro Common AltF11 (Get-PsfMacro e AltF11) 'PSF: Edit recent file'
	#>
	# CtrlShiftF11 -> standard AltF11
	New-FarMacro Common CtrlShiftF11 AltF11 'Far: Open recent file'

	### == Mixed (several areas)

	### (Edit-FarDescription-.ps1) Edit file description: Shell: current item; Editor or Viewer: opened file
	$m = @{
		Name = 'AltZ'
		Sequence = Get-PsfMacro t
		Description = 'PSF: Edit description'
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Viewer @m

	### TabExpansion in any editor line
	$m = @{
		Name = 'F9'
		Sequence = Get-PsfMacro 7
		Description = 'PSF: TabExpansion'
	}
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
	$m.Sequence = Get-CommandLineMacro $m.Sequence
	New-FarMacro Common @m -CommandLine 1

	### (Complete-Word-.ps1) Command line: from history; Editor: from file; Dialog: from edit box history
	$m = @{
		Name = 'CtrlSpace'
		Sequence = Get-PsfMacro c
		Description = 'PSF: Complete word'
	}
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
	$m.Sequence = Get-CommandLineMacro $m.Sequence
	New-FarMacro Common @m -CommandLine 1

	### (Set-Selection-.ps1) Change selected text to lower case
	$m = @{
		Name = 'CtrlU'
		Sequence = Get-PsfMacro l
		Description = 'PSF: Selected text to lower case'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m

	### (Set-Selection-.ps1) Change selected text to upper case
	$m = @{
		Name = 'CtrlShiftU'
		Sequence = Get-PsfMacro u
		Description = 'PSF: Selected text to upper case'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m

	### == Shell only

	### Quit Far
	New-FarMacro Shell F10 (Get-PsfMacro '1 "$Far.Quit()" Enter' F10) 'PSF: Quit Far'

	### Easy prefix: space expands empty command line to '>: '
	New-FarMacro Shell Space '> : Space' 'PSF: Easy prefix' -CommandLine 0

	### Easy invoke: type and run without prefix (Invoke selected code)
	New-FarMacro Shell ShiftSpace (Get-PsfMacro 2) 'PSF: Easy invoke' -CommandLine 1

	### Power panel menu
	New-FarMacro Shell Ctrl= (Get-PsfMacro 6) 'PSF: Power panel menu'

	### (Go-Head-.ps1) Go to head file item (e.g. useful after [CtrlF5] to find the newest file)
	New-FarMacro Shell CtrlShiftF5 (Get-PsfMacro h) 'PSF: Go to panel head item'

	### CtrlC to copy selected text
	New-FarMacro Shell CtrlC 'CtrlIns' 'Copy selected text' -SelectedText 1

	### Open recent folder in the panel
	# Call Show-History-.ps1 by PSF or fallback
	New-FarMacro Shell AltF12 (Get-PsfMacro n AltF12) 'PSF: Open recent folder'
	# CtrlShiftF12 -> standard AltF12
	New-FarMacro Shell CtrlShiftF12 AltF12 'Far: Open recent folder'

	### (Search-Regex-.ps1) Backgroung search in files with dynamic results in the panel
	New-FarMacro Shell CtrlShiftF7 (Get-PsfMacro x) 'PSF: Search regex in files'

	### == Editor only

	### (Invoke-Editor-.ps1) Invoke a file from the editor
	New-FarMacro Editor CtrlF5 (Get-PsfMacro '= f') 'PSF: Invoke a file from the editor'

	### (Indent-Selection-.ps1) Indent selected line(s)
	New-FarMacro Editor Tab (Get-PsfMacro i) 'PSF: Indent selected line(s)' -SelectedText 1

	### (Indent-Selection-.ps1) Outdent selected line(s)
	New-FarMacro Editor ShiftTab (Get-PsfMacro o) 'PSF: Outdent selected line(s)' -SelectedText 1

	### (Reindent-Selection-.ps1) Reindent selected\current line(s)
	New-FarMacro Editor AltF8 (Get-PsfMacro r) 'PSF: Reindent selected\current line(s)'

	### (Reformat-Selection-.ps1) Reformat selected\current line(s)
	New-FarMacro Editor CtrlShiftF8 (Get-PsfMacro f) 'PSF: Reformat selected\current line(s)'

	### Bookmarks
	New-FarMacro Editor Ctrl- 'bm.prev()' 'Go to the previous stack bookmark'
	New-FarMacro Editor CtrlShift- 'bm.next()' 'Go to the next stack bookmark'
	New-FarMacro Editor Ctrl= (Get-PsfMacro '1 print("Select-Bookmark-") Enter') 'Show bookmarks'
	New-FarMacro Editor CtrlShift= 'bm.add()' 'Add a new stack bookmark'

	### == Native (not PSF) macros

	### Go to text selection left edge
	$m = @{
		Name = 'Left'
		Sequence = "Editor.Sel(1,0) Editor.Sel(4)"
		Description = 'Go to text selection left edge'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m

	### Go to text selection right edge
	$m = @{
		Name = 'Right'
		Sequence = "Editor.Sel(1,1) Editor.Sel(4)"
		Description = 'Go to text selection right edge'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
))
