<#PSScriptInfo
.DESCRIPTION Updates Far Manager and standard plugins.
.VERSION 1.0.0
.AUTHOR Roman Kuzmin
.COPYRIGHT (c) Roman Kuzmin
.TAGS Download Update FarManager
.GUID 184e65a5-08bc-4cfa-b1b4-fe659e62ed66
.PROJECTURI https://github.com/nightroman/FarNet
.LICENSEURI https://github.com/nightroman/FarNet/blob/main/LICENSE
#>

<#
.Synopsis
	Updates Far Manager and standard plugins.
	Author: Roman Kuzmin

.Description
	The script updates Far Manager and standard plugins using packages from
	https://github.com/FarGroup/FarManager/releases
	.msi (default) or .7z (requires 7z in the path)

	If Far Manager is running the script prompts to exit its running instances.
	Thus, do not run in Far Manager console. But you may start from there using
	"start" or [ShiftEnter] in the command line. In this case parameter FarHome
	may be omitted, $env:FARHOME is used.

	$HOME is used for downloaded archives. Old versions are not deleted by
	default, use MaxVersions in order to change.

	The script gets the latest web asset name. If the file already exists the
	script stops. Otherwise the file is downloaded and extracted to FarHome.

	Then the script removes plugins that did not exist and some files that did
	not exist. See $UnusedFiles below, they include .hlf, .lng, .map, etc.

	The script also prints existing extra files not found in the package,
	either added by you or retired by the Far Manager team. In the latter
	case you may want to delete extras.

.Notes
	- For .7z packages 7z should be available in the system path.
	- "%TEMP%\FarManager.extracted" is used as temp directory.

.Parameter FarHome
		Far Manager directory.
		Default: $env:FARHOME

.Parameter Platform
		Platform: "x64" or "x86" / "Win32".
		Default: Value from "Far.exe".

.Parameter Archive
		Tells to use this file instead of downloading.

.Parameter PackageType
		Package file type: "msi" or "7z".
		Default: "msi" for downloads, else Archive file extension.

.Parameter MaxVersions
		Tells how many latest archive versions per the specified Platform
		and PackageType to keep in $HOME. Default: [int]::MaxValue, keep
		all versions.

		0 tells to remove all downloaded archives, not recommended, you lose
		the ability of auto-updates, i.e. downloading only newer versions.
#>

[CmdletBinding()]
param(
	[string]$FarHome = $env:FARHOME
	,
	[ValidateSet('x64', 'x86', 'Win32')]
	[string]$Platform
	,
	[string]$Archive
	,
	[ValidateSet('msi', '7z')]
	[string]$PackageType
	,
	[ValidateRange(0, [int]::MaxValue)]
	[int]$MaxVersions = [int]::MaxValue
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}

### Files to remove after updates if they did not exist.
$UnusedFiles = '*.hlf', '*.lng', '*.cmd', '*.map', 'changelog', 'File_id.diz'

function archive_pattern([string]$Platform, [string]$PackageType) {
	"^Far\.$Platform\.(\d+\.\d+\.\d+\.\d+)\.\w+\.$PackageType$"
}

function get_archives([string]$Platform, [string]$PackageType) {
	$pattern = archive_pattern $Platform $PackageType
	foreach($_ in Get-ChildItem $HOME -File) {
		if ($_.Name -match $pattern) {
			[PSCustomObject]@{
				File = $_
				Version = [version]$Matches[1]
			}
		}
	}
}

function get_old_versions([string]$Platform, [string]$PackageType, [int]$MaxVersions) {
	$files = @(get_archives $Platform $PackageType)
	$nRemove = $files.Count - $MaxVersions
	if ($nRemove -ge 1) {
		$files | Sort-Object Version | Select-Object -First $nRemove
	}
}

if ($MyInvocation.InvocationName -eq '.') {
	return
}

### FarHome
if ($FarHome) {
	$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
}
if (![System.IO.Directory]::Exists($FarHome)) {
	throw "Parameter FarHome: directory not found: '$FarHome'."
}

### Platform
if (!$Platform) {
	if (!($exe = Get-Item -LiteralPath "$FarHome\Far.exe" -ErrorAction 0) -or ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b')) {
		throw "Cannot get platform info from Far.exe.`nSpecify parameter Platform."
	}
	$Platform = $Matches[1]
}

### PackageType
if (!$PackageType) {
	$PackageType = if ($Archive) {[System.IO.Path]::GetExtension($Archive).TrimStart('.')} else {'msi'}
}

### download
if ($Archive) {
	$Archive = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Archive)
	if (![System.IO.File]::Exists($Archive)) {
		throw "Parameter Archive: file not found: '$Archive'."
	}
}
else {
	Write-Host -ForegroundColor Cyan "Getting latest from 'https://github.com/FarGroup/FarManager/releases'..."
	$url = 'https://api.github.com/repos/FarGroup/FarManager/releases/latest'
	$ProgressPreference = 0

	# fetch asset meta
	$res = Invoke-RestMethod -Uri $url
	$Platform = if ($Platform -eq 'x64') {'x64'} else {'x86'}
	$pattern = archive_pattern $Platform $PackageType
	$asset = @($res.assets.Where({ $_.name -match $pattern }))
	if ($asset.Count -ne 1) {
		throw "Cannot find expected download assets."
	}

	# check existing file
	$fileName = $Matches[0]
	$Archive = "$HOME\$fileName"
	if ([System.IO.File]::Exists($Archive)) {
		Write-Host -ForegroundColor Cyan "Archive exists: '$Archive'.`nUse it as the parameter Archive to extract."
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

### extract files
Write-Host -ForegroundColor Cyan "Extracting from '$Archive'..."
$plugins1 = [System.IO.Directory]::GetDirectories("$FarHome\Plugins")
$files1 = Get-ChildItem $FarHome -Force -Recurse -File -Name -Include $UnusedFiles

# extract
$extractDir = "$env:TEMP\FarManager.extracted"
if (Test-Path -LiteralPath $extractDir) {
	 Remove-Item -LiteralPath $extractDir -Force -Recurse
}
if ($PackageType -eq 'msi') {
	$p = Start-Process msiexec ('/a "{0}" /qn TARGETDIR="{1}"' -f $Archive, $extractDir) -Wait -PassThru
	if ($p.ExitCode) {throw "Extracting files exit code: $($p.ExitCode)."}
	$fromDir = "$extractDir\Far Manager"
}
elseif($PackageType -eq '7z') {
	& 7z x $Archive "-o$extractDir" -aoa
	$fromDir = $extractDir
}
else {
	throw "Unknown package type: '$PackageType'."
}

# copy
if (![System.IO.Directory]::Exists($fromDir)) {throw "Extracted directory not found: '$fromDir'."}
robocopy $fromDir $FarHome /S /NDL /NFL
if ($LASTEXITCODE -notin (0..3)) {throw "robocopy exit code: $LASTEXITCODE."}

### remove not used plugins
Write-Host -ForegroundColor Cyan "Removing not used plugins..."
$plugins2 = [System.IO.Directory]::GetDirectories("$FarHome\Plugins")
foreach($plugin in $plugins2) {
	if ($plugins1 -notcontains $plugin) {
		Write-Host "Removing plugin '$plugin'"
		[System.IO.Directory]::Delete($plugin, $true)
	}
}

### remove not used files
Write-Host -ForegroundColor Cyan "Removing not used files..."
$files2 = Get-ChildItem $FarHome -Force -Recurse -File -Name -Include $UnusedFiles
foreach($file in $files2) {
	if ($files1 -notcontains $file) {
		Write-Host "Removing file '$file'"
		[System.IO.File]::Delete("$FarHome\$file")
	}
}

### clean versions
if ($versions = @(get_old_versions -Platform $Platform -PackageType $PackageType -MaxVersions $MaxVersions)) {
	Write-Host -ForegroundColor Cyan "Removing old versions ($($versions.Count))..."
	$versions | Select-Object -ExpandProperty File | Remove-Item
}

### check extra items
Write-Host -ForegroundColor Cyan "Checking extra items..."
$fromNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
Get-ChildItem -LiteralPath $fromDir -Force -Recurse -Name | .{process{ $null = $fromNames.Add($_) }}
$toNames = @(
	Get-ChildItem -LiteralPath $FarHome -Force -Name
	Get-ChildItem -LiteralPath "$FarHome\Plugins" -Force -Name | .{process{ "Plugins\$_" }}
	foreach($name in Get-ChildItem -LiteralPath "$fromDir\Plugins" -Directory -Name) {
		if ([System.IO.Directory]::Exists("$FarHome\Plugins\$name")) {
			Get-ChildItem -LiteralPath "$FarHome\Plugins\$name" -Force -Recurse -Name | .{process{ "Plugins\$name\$_" }}
		}
	}
)
$nExtra = 0
foreach($_ in $toNames) {
	if (!$fromNames.Contains($_) -and $_ -notmatch '\.chw$') {
		Write-Host $_
		++$nExtra
	}
}

### clean
Remove-Item -LiteralPath $extractDir -Force -Recurse

### summary
Write-Host -ForegroundColor Cyan "$nExtra extra items."
Write-Host -ForegroundColor Green "Update succeeded."
