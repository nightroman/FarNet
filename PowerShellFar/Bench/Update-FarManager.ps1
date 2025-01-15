<#
.Synopsis
	Updates Far Manager and standard plugins.
	Author: Roman Kuzmin

.Description
	The script updates Far Manager and standard plugins.

	Requires:
	- 7z.exe in the path or as a wrapper command.

	If Far Manager is running the script prompts to exit its running instances.
	Thus, do not run the script in Far Manager. But it is still useful to start
	the script from Far Manager using `start` command or [ShiftEnter] in the
	command line. In this case you do not have to set the parameter FARHOME.

	$HOME directory is the destination for downloaded archives. Old files are
	not deleted. Keep the last downloaded archive there, the script downloads
	only newer archives.

	The script gets the latest web asset name. If the file already exists the
	script exits else it downloads the archive and extracts everything to
	FARHOME. Then it removes plugin folders that did not exist before.

	The script also shows some existing items not found in the archive.

.Parameter FARHOME
		Far Manager directory. Default: %FARHOME%.

.Parameter Platform
		Platform: x64 or x86|Win32. Default: from Far.exe file info.

.Parameter Archive
		Already downloaded archive to be used.

.Example
	ps: Start-Process powershell '-NoExit Update-FarManager'; $Far.Quit()

	Update the current Far Manager in a new console and close the current.
#>

[CmdletBinding()]
param(
	[string]$FARHOME = $env:FARHOME
	,
	[ValidateSet('x64', 'x86', 'Win32')]
	[string]$Platform
	,
	[string]$Archive
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1
if ($Host.Name -ne 'ConsoleHost') {
	Write-Error "Please invoke by the console host."
}

# Files to be removed after updates if they did not exist.
$NotUsedFiles = '*.hlf', '*.lng', '*.cmd', 'changelog', 'File_id.diz'

### FARHOME
if ($FARHOME) {
	$FARHOME = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FARHOME)
}
if (![System.IO.Directory]::Exists($FARHOME)) {
	Write-Error "Parameter FARHOME: missing directory '$FARHOME'."
}

### Platform
if (!$Platform) {
	if (!($exe = Get-Item -LiteralPath "$FARHOME\Far.exe" -ErrorAction 0) -or ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b')) {
		Write-Error "Cannot get platform info from Far.exe.`nSpecify the parameter Platform."
	}
	$Platform = $Matches[1]
}

### download
if ($Archive) {
	$Archive = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Archive)
	if (![System.IO.File]::Exists($Archive)) {
		Write-Error "Missing file: $Archive"
	}
}
else {
	Write-Host -ForegroundColor Cyan "Getting the latest from 'https://github.com/FarGroup/FarManager/releases'..."
	$url = 'https://api.github.com/repos/FarGroup/FarManager/releases/latest'
	$ProgressPreference = 0

	# fetch asset meta
	$res = Invoke-RestMethod -Uri $url
	$bit = if ($Platform -eq 'x64') {'x64'} else {'x86'}
	$asset = @($res.assets.where{ $_.name -match "^Far\.$bit\.\d+\.\d+\.\d+\.\d+\.\w+\.7z$" })
	if ($asset.Count -ne 1) {
		Write-Error "Cannot find expected download assets."
	}

	# check existing file
	$fileName = $Matches[0]
	$Archive = "$HOME\$fileName"
	if ([System.IO.File]::Exists($Archive)) {
		Write-Host -ForegroundColor Cyan "The archive exists: '$Archive'.`nUse it as the parameter Archive to extract."
		return
	}

	# download
	Write-Host -ForegroundColor Cyan "Downloading '$Archive'..."
	$url = $asset.browser_download_url
	Invoke-WebRequest -Uri $url -OutFile $Archive
}

### exit running
Write-Host -ForegroundColor Cyan "Waiting for Far Manager exit..."
Wait-Process Far -ErrorAction 0

### extract all
Write-Host -ForegroundColor Cyan "Extracting from '$Archive'..."
$plugins1 = [System.IO.Directory]::GetDirectories("$FARHOME\Plugins")
$files1 = foreach ($_ in $NotUsedFiles) { [System.IO.Directory]::GetFiles($FARHOME, $_) }
& 7z.exe x $Archive "-o$FARHOME" '-aoa'
if ($LastExitCode) {
	Write-Error "Error on extracting files."
}

### remove not used plugins
Write-Host -ForegroundColor Cyan "Removing not used plugins..."
$plugins2 = [System.IO.Directory]::GetDirectories("$FARHOME\Plugins")
foreach ($plugin in $plugins2) {
	if ($plugins1 -notcontains $plugin) {
		Write-Host "Removing $plugin"
		[System.IO.Directory]::Delete($plugin, $true)
	}
}

### remove not used files
Write-Host -ForegroundColor Cyan "Removing not used files..."
$files2 = foreach ($_ in $NotUsedFiles) {
	[System.IO.Directory]::GetFiles($FARHOME, $_)
}
foreach ($file in $files2) {
	if ($files1 -notcontains $file) {
		Write-Host "Removing $file"
		[System.IO.File]::Delete($file)
	}
}

### check extra items
Write-Host -ForegroundColor Cyan "Checking extra items..."
$pathsInArchive = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
& 7z.exe l $Archive -slt | .{process{ if ($_ -match '^Path = (.+)') { $null = $pathsInArchive.Add($matches[1]) } }}
$pathsInFar = @(
	Get-ChildItem -LiteralPath $FARHOME -Force -Name -ErrorAction 0
	Get-ChildItem -LiteralPath "$FARHOME\Plugins" -Force -Name -ErrorAction 0 | .{process{ "Plugins\$_" }}
	foreach ($path in $pathsInArchive) {
		if ($path -match '^Plugins\\\w+$|^[^\\]+$' -and $path -ne 'Plugins' -and [System.IO.Directory]::Exists("$FARHOME\$path")) {
			Get-ChildItem -LiteralPath "$FARHOME\$path" -Force -Recurse -Name -ErrorAction 0 | .{process{ "$path\$_" }}
		}
	}
)
$nExtra = 0
foreach($_ in $pathsInFar) {
	if (!$pathsInArchive.Contains($_) -and $_ -notmatch '\.chw$') {
		Write-Host $_
		++$nExtra
	}
}

Write-Host -ForegroundColor Cyan "$nExtra extra items."
Write-Host -ForegroundColor Green "Update succeeded."
