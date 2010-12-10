
<#
.SYNOPSIS
	Installs typical RightControl module macros.

.DESCRIPTION
	This script installs 6 macros in the common area:
		CtrlLeft/Right
		CtrlShiftLeft/Right
		CtrlBS/Del
	and 4 editor macros:
		CtrlAltLeft/Right,
		Home/ShiftHome.

	It also installs two workaround macros ShiftLeft/Right in Common.
	They should be removed when Mantis 1465 is resolved.

	Note: Home/ShiftHome can be used in the common area, too, but this script
	installs them in the editor only. Smart home is not really useful for one
	line editors where text normally does not start with spaces.
#>

Import-Module FarMacro

function Get-EditorMacro($key)
{
@'
F11
$If (Menu.Select("RightControl", 3) > 0)
	Enter
'@ + " $key" + @'

$Else
	Esc $AKey
$End
'@
}

function Get-CommonMacro($key)
{
@'
$If (AutoCompletion)
	Esc
$End
$If (Editor || Dialog || ((Shell || Info || QView || Tree) && !CmdLine.Empty))
	F11
	$If (Menu.Select("RightControl", 3) > 0)
		Enter
'@ + " $key" + @'

	$Else
		Esc $AKey
	$End
$Else
	$AKey
$End
'@
}

function Get-WorkaroundMacro($key)
{
@'
$If (AutoCompletion)
	Esc
$End
$If (Dialog || ((Shell || Info || QView || Tree) && !CmdLine.Empty))
	F11
	$If (Menu.Select("RightControl", 3) > 0)
		Enter
'@ + " $key" + @'

	$Else
		Esc $AKey
	$End
$Else
	$AKey
$End
'@
}

### Install all macros
$Far.Macro.Install($(
	# Editor only
	New-FarMacro Editor CtrlAltLeft (Get-EditorMacro 7) 'RightControl: vertical left'
	New-FarMacro Editor CtrlAltRight (Get-EditorMacro 8) 'RightControl: vertical right'
	New-FarMacro Editor Home (Get-EditorMacro h) 'RightControl: go to smart home'
	New-FarMacro Editor ShiftHome (Get-EditorMacro s) 'RightControl: select to smart home'
	# Common for editor, dialog, cmdline
	New-FarMacro Common CtrlLeft (Get-CommonMacro 1) 'RightControl: step left'
	New-FarMacro Common CtrlRight (Get-CommonMacro 2) 'RightControl: step right'
	New-FarMacro Common CtrlShiftLeft (Get-CommonMacro 3) 'RightControl: select left'
	New-FarMacro Common CtrlShiftRight (Get-CommonMacro 4) 'RightControl: select right'
	New-FarMacro Common CtrlBS (Get-CommonMacro 5) 'RightControl: delete left'
	New-FarMacro Common CtrlDel (Get-CommonMacro 6) 'RightControl: delete right'
	# Common for dialog, cmdline (workaround)
	New-FarMacro Common ShiftLeft (Get-WorkaroundMacro 7) 'RightControl: workaround left'
	New-FarMacro Common ShiftRight (Get-WorkaroundMacro 8) 'RightControl: workaround right'
))
