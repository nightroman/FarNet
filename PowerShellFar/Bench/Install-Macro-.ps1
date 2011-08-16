
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

# import the module and install the FarNet constant
Import-Module FarMacro
$Far.Macro.InstallConstant('FarNet', 0xcd)

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

	### Decrease/Increase font size [CtrlMsWheelDown] [CtrlMsWheelUp]
	New-FarMacro Common CtrlMsWheelDown 'F11 $If (Menu.Select(".NET", 2) > 0) Enter c d $End' 'FarNet: Decrease font size'
	New-FarMacro Common CtrlMsWheelUp   'F11 $If (Menu.Select(".NET", 2) > 0) Enter c i $End' 'FarNet: Increase font size'

	### Favorites menu [CtrlShiftL]
	New-FarMacro Common CtrlShiftL 'CallPlugin(FarNet, ">: Menu-Favorites-.ps1")' 'PSF: Favorites'

	### PowerShellFar command history [AltF10]
	# AltF10 in panels disables questionable folder tree.
	New-FarMacro Common AltF10 'CallPlugin(FarNet, ":>: $Psf.ShowHistory()")' 'PSF: Command history' -EnableOutput

	### Search Google (requires the script)
	New-FarMacro Common CtrlShiftG -Description 'PSF: Search Google' -EnableOutput @'
CallPlugin(FarNet, ":>: Search-Google.ps1 (Read-Host 'Search')")
'@

	### == Mixed (several areas)

	### (Edit-FarDescription-.ps1) Edit file description: Shell: current item; Editor or Viewer: opened file [CtrlShiftD]
	$m = @{
		Name = 'CtrlShiftD'
		Sequence = Get-PsfMacro t
		Description = 'PSF: Edit description'
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Viewer @m

	### TabExpansion in any editor line [F9]
	$m = @{
		Name = 'F9'
		Sequence = Get-PsfMacro 7
		Description = 'PSF: TabExpansion'
	}
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
	$m.Sequence = Get-CommandLineMacro $m.Sequence
	New-FarMacro Common @m -CommandLine 1

	### (Complete-Word-.ps1) Command line: from history; Editor: from file; Dialog: from edit box history [CtrlSpace]
	$m = @{
		Name = 'CtrlSpace'
		Sequence = Get-PsfMacro c
		Description = 'PSF: Complete word'
	}
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
	$m.Sequence = Get-CommandLineMacro $m.Sequence
	New-FarMacro Common @m -CommandLine 1

	### (Set-Selection-.ps1) Change selected text to lower case [CtrlU]
	$m = @{
		Name = 'CtrlU'
		Sequence = Get-PsfMacro l
		Description = 'PSF: Selected text to lower case'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m

	### (Set-Selection-.ps1) Change selected text to upper case [CtrlShiftU]
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

	### Decrease/Increase left column [CtrlAltLeft] [CtrlAltRight]
	New-FarMacro Shell CtrlAltLeft -Description 'FarNet: Decrease left column' @'
F11
$If (Menu.Select(".NET", 2) > 0)
	Enter P
	$If (Menu.Select("Decrease left column", 0) > 0)
		Enter
	$Else
		Esc
	$End
$End
'@
	New-FarMacro Shell CtrlAltRight -Description 'FarNet: Increase left column' @'
F11
$If (Menu.Select(".NET", 2) > 0)
	Enter P
	$If (Menu.Select("Increase left column", 0) > 0)
		Enter
	$Else
		Esc
	$End
$End
'@

	### Quit Far [F10]
	New-FarMacro Shell F10 '$If (CallPlugin(FarNet, ">: $Far.Quit()")) $Else F10 $End' 'PSF: Quit Far'

	### Easy invoke: type and run without prefix (Invoke selected code) [ShiftSpace]
	New-FarMacro Shell ShiftSpace (Get-PsfMacro 2) 'PSF: Easy invoke' -CommandLine 1

	### Power panel menu [Ctrl=]
	New-FarMacro Shell Ctrl= (Get-PsfMacro 6) 'PSF: Power panel menu'

	### (Go-Head-.ps1) Go to head file item (e.g. useful after [CtrlF5] to find the newest file) [CtrlShiftF5]
	New-FarMacro Shell CtrlShiftF5 (Get-PsfMacro h) 'PSF: Go to panel head item'

	### Open recent folder in the panel [AltF12] [CtrlShiftF12]
	# Call Show-History-.ps1 by PSF or fallback
	New-FarMacro Shell AltF12 (Get-PsfMacro n AltF12) 'PSF: Open recent folder'
	# CtrlShiftF12 -> standard AltF12
	New-FarMacro Shell CtrlShiftF12 AltF12 'Far: Open recent folder'

	### (Search-Regex-.ps1) Backgroung search in files with dynamic results in the panel [CtrlShiftF7]
	New-FarMacro Shell CtrlShiftF7 (Get-PsfMacro x) 'PSF: Search regex in files'

	### == Editor only

	### (Invoke-Editor-.ps1) Invoke a file from the editor [CtrlF5]
	New-FarMacro Editor CtrlF5 (Get-PsfMacro '= f') 'PSF: Invoke a file from the editor'

	### (Indent-Selection-.ps1) Indent selected line(s) [Tab]
	New-FarMacro Editor Tab (Get-PsfMacro i) 'PSF: Indent selected line(s)' -SelectedText 1

	### (Indent-Selection-.ps1) Outdent selected line(s) [ShiftTab]
	New-FarMacro Editor ShiftTab (Get-PsfMacro o) 'PSF: Outdent selected line(s)' -SelectedText 1

	### (Reindent-Selection-.ps1) Reindent selected\current line(s) [AltF8]
	New-FarMacro Editor AltF8 (Get-PsfMacro r) 'PSF: Reindent selected\current line(s)'

	### (Reformat-Selection-.ps1) Reformat selected\current line(s) [CtrlShiftF8]
	New-FarMacro Editor CtrlShiftF8 (Get-PsfMacro f) 'PSF: Reformat selected\current line(s)'

	### Bookmarks [Ctrl=] [CtrlShift=] [Ctrl-] [CtrlShift-]
	New-FarMacro Editor Ctrl= 'CallPlugin(FarNet, ">: Select-Bookmark-")' 'Show bookmarks'
	New-FarMacro Editor CtrlShift= 'bm.add()' 'Add a new stack bookmark'
	New-FarMacro Editor Ctrl- 'bm.prev()' 'Go to the previous stack bookmark'
	New-FarMacro Editor CtrlShift- 'bm.next()' 'Go to the next stack bookmark'

	### == Native Far Manager macros

	### Easy prefix: space expands empty command line to '>: ' [Space]
	New-FarMacro Shell Space '> : Space' 'PSF: Easy prefix' -CommandLine 0

	### CtrlC to copy selected text [CtrlC]
	New-FarMacro Shell CtrlC 'CtrlIns' 'Copy selected text' -SelectedText 1

	### Go to text selection left edge [Left]
	$m = @{
		Name = 'Left'
		Sequence = "Editor.Sel(1,0) Editor.Sel(4)"
		Description = 'Go to text selection start'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m

	### Go to text selection right edge [Right]
	$m = @{
		Name = 'Right'
		Sequence = "Editor.Sel(1,1) Editor.Sel(4)"
		Description = 'Go to text selection end'
		SelectedText = 1
	}
	New-FarMacro Shell @m
	New-FarMacro Editor @m
	New-FarMacro Dialog @m
))
