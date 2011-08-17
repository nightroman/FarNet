
param
(
	$FARHOME = $env:FARHOME
	,
	[switch]$FarMacro
	,
	[switch]$PowerShellFar
)

function ScriptRoot { Split-Path $MyInvocation.ScriptName }

Add-Type -Path "$FARHOME\FarNet\FarNet.dll"

if ($FarMacro) {
	Import-Module Helps
	Import-Module "$FARHOME\FarNet\Modules\PowerShellFar\Modules\FarMacro\FarMacro.dll"
	Convert-Helps "$(ScriptRoot)\Modules\FarMacro\FarMacro.dll-Help.ps1" "$FARHOME\FarNet\Modules\PowerShellFar\Modules\FarMacro\FarMacro.dll-Help.xml"
}

if ($PowerShellFar) {
	Add-Type -Path "$FARHOME\FarNet\FarNet.Settings.dll"
	Add-Type -Path "$FARHOME\FarNet\FarNet.Tools.dll"
	Add-Type -Path "$FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.dll"
	$ps = [Management.Automation.PowerShell]::Create()
	$configuration = [Management.Automation.Runspaces.RunspaceConfiguration]::Create()
	[PowerShellFar.Zoo]::Initialize($configuration)
	$ps.Runspace = [Management.Automation.Runspaces.RunspaceFactory]::CreateRunspace($configuration)
	$ps.Runspace.Open()
	$null = $ps.AddScript(@"
Import-Module Helps
Convert-Helps "$(ScriptRoot)\Commands\PowerShellFar.dll-Help.ps1" "$FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.dll-Help.xml"
"@)
	$ps.Invoke()
}
