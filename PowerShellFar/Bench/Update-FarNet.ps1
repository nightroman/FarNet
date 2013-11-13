
<#
.Synopsis
	Updates FarNet products selectively from the project site.
	Author: Roman Kuzmin

.Description
	Command 7z has to be available, e.g. 7z.exe in the system path.

	Ensure that Far Manager being updated has no running instances.

	-ArchiveHome is the destination for downloaded archives. Old files are not
	deleted. Keep the last downloaded archives there, the script downloads only
	new archives.

	On updating from the archives the script simply extracts files and replace
	existing same files with no warnings. Existing extra files are not deleted:
	thus, read History.txt on updates, you may want to remove some old files.

	<Archive>\Install.txt files show what is updated from <Archive>.

.Parameter FARHOME
		Far Manager directory. Default: %FARHOME%.
.Parameter Platform
		Platform: x64 or x86|Win32. Default: from Far.exe file info.
.Parameter ArchiveHome
		Downloaded archives directory. Default: $HOME.
.Parameter ArchiveNames
		Archive names. Default: latest from the project site.
.Parameter Force
		Tells to update from already downloaded archives.
.Parameter All
		Tells to update all.

.Example
	ps: Start-Process PowerShell.exe '-NoExit Update-FarNet'; $Far.Quit()

	Update the current Far Manager in a new console and close the current.
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param
(
	[string]
	$FARHOME = $env:FARHOME,
	[string][ValidateSet('x64', 'x86', 'Win32')]
	$Platform,
	[string][ValidateScript({[System.IO.Directory]::Exists($_)})]
	$ArchiveHome = $HOME,
	[string[]]
	$ArchiveNames,
	[switch]
	$Force,
	[switch]
	$All
)

Set-StrictMode -Version 2
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

### to download if not yet
# see Update-FarManager.ps1
$wc = New-Object Net.WebClient

### download the archive names
if (!$ArchiveNames) {
	$URL = "http://farnet.googlecode.com/svn/trunk/Get-Version.ps1"
	Write-Host -ForegroundColor Cyan "Getting version from '$URL'..."
	$initext = $wc.DownloadString($URL)
	Invoke-Expression $initext
}

### confirm each archive name
$ArchiveNames = foreach($name in $ArchiveNames) {
	if ($All -or $PSCmdlet.ShouldProcess($name, "Download and/or update")) {
		$name
	}
}

### download missing archives
$done = 0
foreach($name in $ArchiveNames) {
	$path = "$ArchiveHome\$name"
	if ([IO.File]::Exists($path)) {
		Write-Host -ForegroundColor Cyan "The archive '$path' already exists."
	}
	else {
		$URL = "http://farnet.googlecode.com/files/$name"
		Write-Host -ForegroundColor Cyan "Downloading '$name' from $URL..."
		$wc.DownloadFile($URL, $path)
		++$done
	}
}

if (!$Force -and $done -eq 0) {
	Write-Host -ForegroundColor Cyan "All archives already exist, use -Force to update from them."
	return
}

### extract from archives
foreach($name in $ArchiveNames) {
	# the archive
	$path = "$ArchiveHome\$name"
	Write-Host -ForegroundColor Cyan "Extracting from '$path'..."

	# extract install list
	& '7z' 'e' $path "-o$($env:TEMP)" '-aoa' 'Install.txt'
	if ($LastExitCode) {Write-Error "Error on extracting Install.txt."}

	# extract using install list
	$install = "$($env:TEMP)\Install.txt"
	& '7z' 'x' $path "-o$FARHOME" '-aoa' "@$install"
	if ($LastExitCode) {Write-Error "Error on extracting files."}
	[System.IO.File]::Delete($install)

	# x64 FarNet
	if ($Platform -eq 'x64' -and $name -like 'FarNet.*') {
		& '7z' 'e' $path "-o$FARHOME\Plugins\FarNet" '-aoa' 'Plugins.x64\FarNet\FarNetMan.dll'
		if ($LastExitCode) {Write-Error "Error on extracting FarNetMan.dll."}
	}
}

### done
Write-Host -ForegroundColor Green "Update succeeded."
