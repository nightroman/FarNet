<#
.Synopsis
	Test completion of variables in the editor manually.
.Notes
	*** The file contains syntax mistakes deliberately.
#>

param($MyParam1)

# see MyParam1 (fixed)
$my

# see $global:GlobalVar1, $GlobalVar1, no `global` (fixed)
$global:GlobalVar1
$g

# see $script:ScriptVar1, $ScriptVar1, no `script` (fixed)
$script:ScriptVar1
$s

# see $private:PrivateVar1, $PrivateVar1
$private:PrivateVar1
$p

# see ($MyParam1 (fixed)
($my

# see $map.Add($MyParam1 (fixed)
$map.Add($my

$global:xGlobalVar1
$global:xGlobalVar2
$script:xScriptVar1
$script:xScriptVar2

# see $global:xGlobalVar1, $global:xGlobalVar2, no $script:...
$global:x

# see $script:xScriptVar1, $script:xScriptVar2, no $global:...
$script:x

# ISE does not work good
$x
