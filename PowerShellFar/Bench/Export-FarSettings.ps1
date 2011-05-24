
<#
.SYNOPSIS
	Exports Far Manager settings.
	Author: Roman Kuzmin

.DESCRIPTION
	This script and Import-FarSettings.ps1 allow to export and import only main
	Far Manger settings that normally should be the same for any machine of the
	same user. Many other settings like history data normally should not be
	transferred. Key Users is also excluded.

	Invoke this script by the console host. Far Manager should not be running
	(this is checked) and this script should not be already running (this is
	not checked).

	Output files:
		Far-Settings.reg
			Output file with exported settings. If settings are not changed
			then this file is not updated.
		Far-Settings.bak
			Backup copy of previous FarSettings.reg, if any.

	Rescue files:
		All-Settings.reg
			Temporary backup of all settings. If export fails then you should
			import this file manually by regedit from explorer.
		Far-Settings.tmp
			Intermediate exported file; it becomes FarSettings.reg if it is
			different otherwise it is just discarded.
#>

param
(
	[string]
	# Directory path for exported and rescue files.
	$Path = '.'
)

$ErrorActionPreference = 'Stop'
$AllSettingsReg = Join-Path $Path All-Settings.reg
$FarSettingsTmp = Join-Path $Path Far-Settings.tmp
$FarSettingsReg = Join-Path $Path Far-Settings.reg
$FarSettingsBak = Join-Path $Path Far-Settings.bak

# directory must exist
if (![IO.Directory]::Exists($Path)) { throw "Directory '$Path' does not exist." }

### Far Manager exit
Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit..."
Wait-Process Far -ErrorAction 0

### export all
Write-Host -ForegroundColor Cyan "Exporting '$AllSettingsReg' (restore on failure manually)..."
cmd /c regedit /e $AllSettingsReg HKEY_CURRENT_USER\Software\Far2

# kill in the registry what is not exported
Push-Location HKCU:\Software\Far2
# kill whole keys
Remove-Item -Recurse -Force -ErrorAction 0 -Path @(
	'Editor\LastPositions'
	'Layout'
	'Panel\Left'
	'Panel\Right'
	'PluginsCache'
	'SavedDialogHistory'
	'SavedFolderHistory'
	'SavedHistory'
	'SavedViewHistory'
	'Users'
	'Viewer\LastPositions'
)
# kill noisy properties
Remove-ItemProperty Plugins\S_And_R -Name search, replace -ErrorAction 0
Pop-Location

### export filtered settings to a temp file
Write-Host -ForegroundColor Cyan "Exporting '$FarSettingsTmp'..."
cmd /c regedit /e $FarSettingsTmp HKEY_CURRENT_USER\Software\Far2

### restore original settings
Write-Host -ForegroundColor Cyan "Importing '$AllSettingsReg'..."
cmd /c regedit /s $AllSettingsReg
[IO.File]::Delete($AllSettingsReg)

# update the file if needed
if (![IO.File]::Exists($FarSettingsReg)) {
	Write-Host -ForegroundColor Cyan "Creating '$FarSettingsReg'..."
	[IO.File]::Move($FarSettingsTmp, $FarSettingsReg)
}
elseif ([IO.File]::ReadAllText($FarSettingsTmp) -cne [IO.File]::ReadAllText($FarSettingsReg)) {
	Write-Host -ForegroundColor Cyan "Updating '$FarSettingsReg'..."
	[IO.File]::Replace($FarSettingsTmp, $FarSettingsReg, $FarSettingsBak)
}
else {
	Write-Host -ForegroundColor Cyan "Removing '$FarSettingsTmp'..."
	[IO.File]::Delete($FarSettingsTmp)
}

# end
Write-Host -ForegroundColor Green "Export succeeded."
