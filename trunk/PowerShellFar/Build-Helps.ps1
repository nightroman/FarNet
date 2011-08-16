
param
(
	$FARHOME = $env:FARHOME
)

Import-Module Helps

function ScriptRoot { Split-Path $MyInvocation.ScriptName }

Add-Type -Path "$FARHOME\FarNet\FarNet.dll"
Import-Module "$FARHOME\FarNet\Modules\PowerShellFar\Modules\FarMacro\FarMacro.dll"

Convert-Helps "$(ScriptRoot)\Modules\FarMacro\FarMacro.dll-Help.ps1" "$FARHOME\FarNet\Modules\PowerShellFar\Modules\FarMacro\FarMacro.dll-Help.xml"
