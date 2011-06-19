
<#
.SYNOPSIS
	Installs RightWords macros.
#>

Import-Module FarMacro
$Far.Macro.Install($(

	# Correct word: common for editor, dialog, and command line
	New-FarMacro Common CtrlShiftSpace -Description 'RightWords: correct word' @'
$If (AutoCompletion)
	Esc
$End
$If (Editor || Dialog || ((Shell || Info || QView || Tree) && !CmdLine.Empty))
	F11 6 1
$End
'@

	# Correct text: for editor
	New-FarMacro Editor CtrlShiftF7 -Description 'RightWords: correct text' @'
F11 6 2
'@

))
