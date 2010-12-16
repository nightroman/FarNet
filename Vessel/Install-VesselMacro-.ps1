
<#
.SYNOPSIS
	Installs Vessel macros.

.DESCRIPTION
	Macros:
	* Common\AltF11 - smart file history instead of standard
	* Common\CtrlShiftF11 - show standard "File view history"
#>

Import-Module FarMacro

$Far.Macro.Install($(
	New-FarMacro Common CtrlShiftF11 AltF11 'Far: File view history'
	New-FarMacro Common AltF11 -Description 'Vessel: Smart file history' @'
F11
$If (Menu.Select("Vessel", 3) > 0)
	Enter 1
$Else
	Esc $AKey
$End
'@

))
