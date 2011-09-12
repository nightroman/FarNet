
<#
.SYNOPSIS
	Updates FarNet products selectively from the project site.
	Author: Roman Kuzmin

.DESCRIPTION
	Command 7z has to be available, e.g. 7z.exe in the system path.

	If Far Manager is running the script prompts you to exit running instances
	and waits until this is done. That is why you should not run the script IN
	Far Manager. On the other hand it is still useful to start the script FROM
	Far Manager (using 'start' command or [ShiftEnter] in the command line), in
	this case you do not have to set the parameter -FARHOME. If -FARHOME is UNC
	then that machine has to be configured for PS remoting.

	-ArchiveHome is the destination for downloaded archives. Old files are not
	deleted. Keep the last downloaded archives there, the script downloads only
	new archives.

	On updating from the archives the script simply extracts files and replace
	existing same files with no warnings. Existing extra files are not deleted:
	thus, read History.txt on updates, you may want to remove some old files.

	<Archive>\Install.txt files show what is updated from <Archive>.

.EXAMPLE
	# This command starts update in a new console and keeps it opened to view
	# the output. Then it tells Far to exit because update will wait for this.
	>: Start-Process powershell.exe "-noexit Update-FarNet"; $Far.Quit()
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param
(
	[string]
	[ValidateScript({[System.IO.Directory]::Exists($_)})]
	# Far directory; needed if %FARHOME% is not defined and its location is not standard.
	$FARHOME = $(if ($env:FARHOME) {$env:FARHOME} else {"C:\Program Files\Far"})
	,
	[string]
	[ValidateSet('x86', 'x64')]
	# Target platform: x86 or x64. Default: depends on the current process.
	$Platform = $(if ([intptr]::Size -eq 4) {'x86'} else {'x64'})
	,
	[string]
	[ValidateScript({[System.IO.Directory]::Exists($_)})]
	# Downloaded archives directory. Default: $HOME.
	$ArchiveHome = $HOME
	,
	[string[]]
	# Archive names. Default: latest from the site.
	$ArchiveNames
	,
	[switch]
	# Tells to update from already downloaded archives.
	$Force
	,
	[switch]
	# Tells to update all.
	$All
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'
if ($Host.Name -ne 'ConsoleHost') { throw "Please, invoke by the console host." }

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
	Write-Host -ForegroundColor Cyan "All the archives already exist, use -Force to update from them."
	return
}

### exit running; use remoting for UNC
if (Test-Path "$FARHOME\Far.exe") {
	$uri = [System.Uri]$FARHOME
	if ($uri.IsUnc) {
		Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit: $($uri.Host)..."
		Invoke-Command -ComputerName $uri.Host { Wait-Process Far -ErrorAction 0 }
	}
	else {
		Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit..."
		Wait-Process Far -ErrorAction 0
	}
}

### extract from archives
foreach($name in $ArchiveNames) {
	# the archive
	$path = "$ArchiveHome\$name"
	Write-Host -ForegroundColor Cyan "Extracting from '$path'..."

	# extract the install list
	& '7z' 'e' $path "-o$($env:TEMP)" '-aoa' 'Install.txt'
	if ($lastexitcode) { throw "7z failed." }

	# extract using the install list
	$install = "$($env:TEMP)\Install.txt"
	& '7z' 'x' $path "-o$FARHOME" '-aoa' "@$install"
	if ($lastexitcode) { throw "7z failed." }
	[System.IO.File]::Delete($install)

	# x64 FarNet
	if ($Platform -eq 'x64' -and $name -like 'FarNet.*') {
		& '7z' 'e' $path "-o$FARHOME\Plugins\FarNet" '-aoa' 'Plugins.x64\FarNet\FarNetMan.dll'
		if ($lastexitcode) { throw "7z failed." }
	}
}

### done
Write-Host -ForegroundColor Green "Update succeeded."
