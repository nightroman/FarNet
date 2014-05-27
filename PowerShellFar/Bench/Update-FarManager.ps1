
<#
.Synopsis
	Updates Far Manager and standard plugins.
	Author: Roman Kuzmin

.Description
	Command 7z has to be available, e.g. 7z.exe in the system path.

	If Far Manager is running the script prompts you to exit running instances
	and waits until this is done. That is why you should not run the script in
	Far Manager. On the other hand it is still useful to start the script from
	Far Manager (using 'start' command or [ShiftEnter] in the command line), in
	this case you do not have to set the parameter FARHOME.

	$HOME directory is the destination for downloaded archives. Old files are
	not deleted. Keep at least the last downloaded archive there, the script
	downloads only new archives, if any.

	The script gets the latest web archive name. If the file already exists the
	script exits. Otherwise it downloads the archive and starts extraction. It
	extracts everything to the home directory and then removes plugin folders
	that did not exist before.

	Finally the script checks and shows extra user items which do not exist in
	the archive, e.g. user plugins and files and standard files excluded now.

.Parameter FARHOME
		Far Manager directory. Default: %FARHOME%.
.Parameter Platform
		Platform: x64 or x86|Win32. Default: from Far.exe file info.
.Parameter Version
		Major Far version: 3 (default) or 2.
.Parameter Archive
		Already downloaded archive to be used.
.Parameter Stable
		Download and update only stable builds.

.Example
	ps: Start-Process PowerShell.exe '-NoExit Update-FarManager'; $Far.Quit()

	Update the current Far Manager in a new console and close the current.
#>

[CmdletBinding()]
param(
	[string]
	$FARHOME = $env:FARHOME,
	[string][ValidateSet('x64', 'x86', 'Win32')]
	$Platform,
	[int][ValidateSet(2, 3)]
	$Version = 3,
	[string]
	$Archive,
	[switch]
	$Stable
)

# Files to be removed after updates if they are originally missing.
$NotUsedFiles = '*.hlf', '*.lng', '*.cmd', 'File_id.diz', 'changelog_eng'

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($Host.Name -ne 'ConsoleHost') {Write-Error "Please, invoke by the console host."}

### FARHOME
if ($FARHOME) {$FARHOME = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FARHOME)}
if (![IO.Directory]::Exists($FARHOME)) {Write-Error "Parameter FARHOME: missing directory '$FARHOME'."}

### Platform
if (!$Platform) {
	if (!($exe = Get-Item -LiteralPath "$FARHOME\Far.exe" -ErrorAction 0) -or ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b')) {
		Write-Error "Cannot get info from Far.exe. Specify parameter Platform."
	}
	$Platform = $matches[1]
}

### download if not yet
#! DownloadFile() depends on IE Tools > Internet Options > Connections > LAN settings
if (!$Archive) {
	### get URL: stable, platform
	$URL = if ($Stable) {"http://www.farmanager.com/files/update$Version.php"} else {"http://www.farmanager.com/nightly/update$Version.php"}
	$URL += if ($Platform -eq 'x64') {'?p=64'} else {'?p=32'}

	### look for updates (request the archive name)
	Write-Host -ForegroundColor Cyan "Looking for updates at '$URL'..."
	$wc = New-Object Net.WebClient
	$initext = $wc.DownloadString($URL)
	if ($initext -notmatch 'arc="(Far[^"]+\.7z)"') {Write-Error "Cannot get archive name from '$ini'."}

	### exit if the archive exists
	$Name = $matches[1]
	$Archive = "$HOME\$Name"
	if ([IO.File]::Exists($Archive)) {
		Write-Host -ForegroundColor Cyan "The archive '$Archive' already exists; use the parameter -Archive to update from it."
		return
	}

	### download the archive
	$URL = $(if ($Stable) {"http://www.farmanager.com/files/$Name"} else {"http://www.farmanager.com/nightly/$Name"})
	Write-Host -ForegroundColor Cyan "Downloading '$Archive' from $URL..."
	$wc.DownloadFile($URL, $Archive)
}

### exit running
Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit..."
Wait-Process Far -ErrorAction 0

### extract all
Write-Host -ForegroundColor Cyan "Extracting from '$Archive'..."
$plugins1 = [System.IO.Directory]::GetDirectories("$FARHOME\Plugins")
$files1 = foreach($_ in $NotUsedFiles) { [System.IO.Directory]::GetFiles($FARHOME, $_) }
& '7z' 'x' $Archive "-o$FARHOME" '-aoa'
if ($LastExitCode) {Write-Error "Error on extracting files."}

### remove not used plugins
Write-Host -ForegroundColor Cyan "Removing not used plugins..."
$plugins2 = [System.IO.Directory]::GetDirectories("$FARHOME\Plugins")
foreach($plugin in $plugins2) {
	if ($plugins1 -notcontains $plugin) {
		Write-Host "Removing $plugin"
		[System.IO.Directory]::Delete($plugin, $true)
	}
}

### remove not used files
Write-Host -ForegroundColor Cyan "Removing not used files..."
$files2 = foreach($_ in $NotUsedFiles) { [System.IO.Directory]::GetFiles($FARHOME, $_) }
foreach($file in $files2) {
	if ($files1 -notcontains $file) {
		Write-Host "Removing $file"
		[System.IO.File]::Delete($file)
	}
}

### check extra items
Write-Host -ForegroundColor Cyan "Checking extra items..."
$nExtra = 0
$inArchive = @{}
.{ & '7z' 'l' $Archive '-slt' | .{process{ if ($_ -match '^Path = (.+)') { $inArchive.Add($matches[1], $null) } }} }
.{
	Get-ChildItem $FarHome -Force -Name -ErrorAction 0
	Get-ChildItem "$FarHome\Plugins" -Force -Name -ErrorAction 0 | .{process{ "Plugins\$_" }}
	foreach($key in $inArchive.Keys) {
		if ($key -match '^Plugins\\\w+$|^[^\\]+$' -and $key -ne 'Plugins' -and [IO.Directory]::Exists("$FARHOME\$key")) {
			Get-ChildItem "$FARHOME\$key" -Recurse -Force -Name -ErrorAction 0 | .{process{ "$key\$_" }}
		}
	}
} | .{process{
	if (!$inArchive.ContainsKey($_)) {
		$_
		++$nExtra
	}
}}

Write-Host -ForegroundColor Cyan "$nExtra extra items."
Write-Host -ForegroundColor Green "Update succeeded."
