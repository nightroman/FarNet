
<#
.Synopsis
	Installs typical RightControl module macros.

.Description
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
	installs them in the editor only. Smart home is not really useful for line
	editors where text normally does not start with spaces.
#>

# import the module and install the FarNet constant
Import-Module FarMacro
$Far.Macro.InstallConstant('FarNet', 0xcd)

function Get-EditorMacro($command)
{
@'
CallPlugin(FarNet, "RightControl:
'@ + $command + @'
")
'@
}

function Get-CommonMacro($command)
{
@'
$If (AutoCompletion)
	Esc
$End
$If (Editor || Dialog || ((Shell || Info || QView || Tree) && !CmdLine.Empty))
	CallPlugin(FarNet, "RightControl:
'@ + $command + @'
")
$Else
	$AKey
$End
'@
}

function Get-WorkaroundMacro($command)
{
@'
$If (AutoCompletion)
	Esc
$End
$If (Dialog || ((Shell || Info || QView || Tree) && !CmdLine.Empty))
	CallPlugin(FarNet, "RightControl:
'@ + $command + @'
")
$Else
	$AKey
$End
'@
}

### Install all macros
$Far.Macro.Install($(
	# Editor only
	New-FarMacro Editor CtrlAltLeft (Get-EditorMacro 'vertical-left') 'RightControl: vertical left'
	New-FarMacro Editor CtrlAltRight (Get-EditorMacro 'vertical-right') 'RightControl: vertical right'
	New-FarMacro Editor Home (Get-EditorMacro 'go-to-smart-home') 'RightControl: go to smart home'
	New-FarMacro Editor ShiftHome (Get-EditorMacro 'select-to-smart-home') 'RightControl: select to smart home'
	# Common for editor, dialog, cmdline
	New-FarMacro Common CtrlLeft (Get-CommonMacro 'step-left') 'RightControl: step left'
	New-FarMacro Common CtrlRight (Get-CommonMacro 'step-right') 'RightControl: step right'
	New-FarMacro Common CtrlShiftLeft (Get-CommonMacro 'select-left') 'RightControl: select left'
	New-FarMacro Common CtrlShiftRight (Get-CommonMacro 'select-right') 'RightControl: select right'
	New-FarMacro Common CtrlBS (Get-CommonMacro 'delete-left') 'RightControl: delete left'
	New-FarMacro Common CtrlDel (Get-CommonMacro 'delete-right') 'RightControl: delete right'
	# Common for dialog, cmdline (workaround)
	New-FarMacro Common ShiftLeft (Get-WorkaroundMacro 'vertical-left') 'RightControl: workaround left'
	New-FarMacro Common ShiftRight (Get-WorkaroundMacro 'vertical-right') 'RightControl: workaround right'
))

### Show demo macros
Get-EditorMacro editor
'-----'
Get-CommonMacro common
'-----'
Get-WorkaroundMacro workaround
