
<#
.SYNOPSIS
	Imports Far Manager settings.
	Author: Roman Kuzmin

.DESCRIPTION
	This script is used with Export-FarSettings.ps1 which exports settings. At
	first Export-FarSettings.ps1 should be called (machine A) then exported
	data are imported by this script (machine B or even the same machine A).

	Invoke this script by the console host. Far Manager should not be running
	(this is checked) and this script should not be already running (this is
	not checked).

	CAUTION:
	Do not import exported .reg files directly (e.g. by running them from
	explorer by regedit). This operation may cause incorrectly merged data.
#>

param
(
	[string]
	# Directory path where settings were previously exported.
	$Path = '.'
)

$ErrorActionPreference = 'Stop'
$FarSettingsReg = Join-Path $Path Far-Settings.reg
$FarSettingsXml = Join-Path $Path Far-Settings.xml

# file must exist
if (![IO.File]::Exists($FarSettingsReg)) { throw "File '$FarSettingsReg' does not exist." }

### Far Manager exit
Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit..."
Wait-Process Far -ErrorAction 0

### get PSF history
$history = @{}
$regkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Far2\Plugins\FarNet.Modules\PowerShellFar.dll\CommandHistory')
if ($regkey) {
	foreach($name in $regkey.GetValueNames()) {
		$history.Add($name, $regkey.GetValue($name))
	}
	$regkey.Close()
}

# kill in the registry some data being imported
Push-Location HKCU:\Software\Far2
Remove-Item -Recurse -ErrorAction 0 -Path `
Associations,
KeyMacros,
PluginHotkeys,
Plugins,
SortGroups,
UserMenu\MainMenu
Pop-Location

### import data
Write-Host -ForegroundColor Cyan "Importing '$FarSettingsReg'..."
cmd /c regedit /s $FarSettingsReg

### merge PSF history
Write-Host -ForegroundColor Cyan "Merging '$FarSettingsXml'..."
if (Test-Path $FarSettingsXml) {
	$history2 = Import-Clixml $FarSettingsXml
	foreach($name in $history2.Keys) {
		$history[$name] = $history2[$name]
	}
}
$regkey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Far2\Plugins\FarNet.Modules\PowerShellFar.dll\CommandHistory')
foreach($name in ($history.Keys | Sort-Object)) {
	$regkey.SetValue($name, $history[$name])
}
$regkey.Close()

# end
Write-Host -ForegroundColor Green "Import succeeded."
