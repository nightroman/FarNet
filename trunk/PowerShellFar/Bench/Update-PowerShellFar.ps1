
<#
.SYNOPSIS
	Updates FarNet, PowerShellFar and API help.
	Author: Roman Kuzmin

.DESCRIPTION
	If Far Manager is running the script prompts you to exit running instances
	and waits until this is done. That is why you should not run the script in
	Far Manager. On the other hand it is still useful to start the script from
	Far Manager (using 'start' command or [ShiftEnter] in the command line), in
	this case you do not have to set the parameter -FARHOME.

	-Archive directory is used for a temp file PowerShellFar.Readme.txt and as
	a destination for downloaded archives. Old files are not deleted. Keep the
	last downloaded archives there, the script downloads only new archives and
	does nothing if they are already downloaded.

	Command 7z has to be available, e.g. 7z.exe in the system path.

	On updating from the archives the script extracts only main files and skips
	optional (e.g. Bench, Extras, and etc.). Your extra files are not deleted
	but remember that extracted files replace existing files with no warnings.

	What exactly is updated in %FARHOME% from the archives:
	Folders:
	-- Lib
	-- Plugins\FarNet
	-- Plugins.NET\PowerShellFar
	Files:
	-- Far.exe.config
	-- Plugins.NET\PowerShellFar.chm

.OUTPUTS
	None if the script fails or does nothing. If the script really updates then
	it returns actually used version number, string X.Y.Z, which can be used by
	a calling script to perform extra steps, e.g. to extract more files from
	PowerShellFar.X.Y.Z.7z archive (Bench scripts, Extras for Colorer).

.EXAMPLE
	# This command is suitable for the Far command line or the user menu. Also,
	# it keeps the console opened, so that you can view the output.
	start powershell -noexit Update-PowerShellFar
#>

param
(
	[Parameter()][string]
	# Far directory; needed if %FARHOME% is not defined and its location is not standard.
	$FARHOME = $(if ($env:FARHOME) {$env:FARHOME} else {"C:\Program Files\Far"})
	,
	[string]
	# Target platform: x86 or x64. Default: depends on the current process.
	$Platform = $(if ([intptr]::Size -eq 4) {'x86'} else {'x64'})
	,
	[string]
	# Downloaded archives directory. Default: %USERPROFILE%.
	$Archive = $env:USERPROFILE
	,
	[string]
	# Version (X.Y.Z). Default: requested from the FarNet site.
	$Version
	,
	[switch]
	# Tells to update from already downloaded archives.
	$Force
)

if ($Host.Name -ne 'ConsoleHost') { throw "Please, invoke by the console host." }
if (![IO.Directory]::Exists($FARHOME)) { throw "Directory not found: '$FARHOME'." }
if (![IO.Directory]::Exists($Archive)) { throw "Directory not found: '$Archive'." }
if (@('x86', 'x64') -notcontains $Platform) { throw "Invalid platform value: '$Platform'." }
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

### to download if not yet
# see notes in Update-FarManager.ps1
$wc = New-Object Net.WebClient

### request version
if (!$Version) {
	# download Readme.txt
	$URL = "http://farnet.googlecode.com/svn/trunk/PowerShellFar/Readme.txt"
	Write-Host -ForegroundColor Cyan @"
Getting version from '$URL'...
"@
	$ini = "$Archive\PowerShellFar.Readme.txt"
	$wc.DownloadFile($URL, $ini)

	# extract version
	$initext = [IO.File]::ReadAllText($ini)
	if ($initext -notmatch 'Version\s+:\s+(\d+\.\d+\.\d+)') { throw "Cannot get version from '$ini'." }
	Remove-Item -LiteralPath $ini -ErrorAction 'Continue'
	$Version = $matches[1]
}

### download missed archives
$Names = @("FarNet.$Version.7z", "PowerShellFar.$Version.7z", "PowerShellFar.doc.$Version.7z")
$Archives = @("$Archive\$($Names[0])", "$Archive\$($Names[1])", "$Archive\$($Names[2])")
$done = 0
for($$ = 0; $$ -lt 3; ++$$) {
	if ([IO.File]::Exists($Archives[$$])) {
		Write-Host -ForegroundColor Cyan "The archive '$($Archives[$$])' already exists."
	}
	else {
		$URL = "http://farnet.googlecode.com/files/$($Names[$$])"
		Write-Host -ForegroundColor Cyan "Downloading '$($Archives[$$])' from $URL..."
		$wc.DownloadFile($URL, $Archives[$$])
		++$done
	}
}
if (!$Force -and $done -eq 0) {
	Write-Host -ForegroundColor Cyan "All the archives already exist, use -Force to update from them."
	return
}

### exit running
Write-Host -ForegroundColor Cyan @"
Waiting for Far Manager exit...
"@
Wait-Process Far -ErrorAction 0

### extract FarNet
Write-Host -ForegroundColor Cyan @"
Extracting from '$($Archives[0])'...
"@
# x86
& '7z' 'x' ($Archives[0]) "-o$FARHOME" '-aoa' 'Far.exe.config' 'Lib' 'Plugins\FarNet'
if ($lastexitcode) { throw "7z failed." }
# x64
if ($Platform -eq 'x64') {
	& '7z' 'e' ($Archives[0]) "-o$FARHOME\Plugins\FarNet" '-aoa' 'Plugins.x64\FarNet\FarNetMan.dll'
	if ($lastexitcode) { throw "7z failed." }
}

### extract PowerShellFar
Write-Host -ForegroundColor Cyan @"
Extracting from '$($Archives[1])'...
"@
& '7z' 'x' ($Archives[1]) "-o$FARHOME" '-aoa' 'Plugins.NET\PowerShellFar'
if ($lastexitcode) { throw "7z failed." }

### extract PowerShellFar.chm
Write-Host -ForegroundColor Cyan @"
Extracting from '$($Archives[2])'...
"@
& '7z' 'x' ($Archives[2]) "-o$FARHOME" '-aoa'
if ($lastexitcode) { throw "7z failed." }

### output update version
$Version

Write-Host -ForegroundColor Green @"
Update succeeded.
"@
