
<#
.Synopsis
	Updates or installs Far Manager NuGet packages.
	Author: Roman Kuzmin

.Description
	Recommendations, not always requirements:
	- Close all running Far Manager instances before updating.
	- Invoke the script by the console host, i.e. PowerShell.exe.

	On updating from the package the script simply extracts files and replace
	existing same files with no warnings. Existing extra files are not deleted.
	Read release notes on updates and remove extra files manually.

.Parameter PackageId
		The package ID, e.g. FarNet, FarNet.PowerShellFar.
		If it is omitted then PackagePath must be specified.
.Parameter PackageVersion
		The existing package version, e.g. 5.0.38.
		If it is omitted then the latest version is taken from NuGet.
.Parameter PackagePath
		Tells to update from the local package and specifies its path.
		Parameters PackageId and PackageVersion are ignored.
.Parameter FarHome
		The Far Manager directory. Default: %FARHOME%. The directory must exist
		but it does not have to be the actual Far Manager home directory, files
		can be extracted to any directory.
.Parameter Platform
		Platform: x64 or x86|Win32. The default is extracted from Far.exe info.
		It is needed for packages with FarHome.x64|x86 folders, for example for
		the package FarNet. FarNet modules normally do not need this parameter.
.Parameter FarPackages
		The directory for downloaded packages.
		Default: "$env:LOCALAPPDATA\FarPackages".

		Keep some last downloaded packages, so that the script can check for
		existing packages and avoid unnecessary downloads and updates.

		This directory is also used as destination for extra files extracted
		from package folders "About" (manuals, change logs, add-ons, and etc.).
		After updates look at directories <package name without extension>.

.Example
	> Update-FarPackage.ps1 FarNet -FarHome <path> [-Platform <x64|x86>]

	This command updates FarNet. The Platform is needed if Far.exe is not in
	<path>. After updating look at extra files at the FarPackage directory:
	FarNet.X.Y.Z contains About-FarNet.htm, FarNetAPI.chm, History.txt.

.Example
	> Update-FarPackage.ps1 FarNet.PowerShellFar -FarHome <path>

	This command updates PowerShellFar. The Platform is not needed.
	After updating look at extra files at the FarPackage directory:
	FarNet.PowerShellFar.X.Y.Z contains About-PowerShellFar.htm, History.txt.
#>

param(
	[Parameter(Position=0)]
	[string]
	$PackageId,
	[Parameter(Position=1)]
	[string]
	$PackageVersion,
	[string]
	$PackagePath,
	[string]
	$FarHome = $env:FARHOME,
	[string][ValidateSet('x64', 'x86', 'Win32')]
	$Platform,
	[string]
	$FarPackages = "$env:LOCALAPPDATA\FarPackages"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# check FarHome
if ($FarHome) { $FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome) }
if (![IO.Directory]::Exists($FarHome)) { Write-Error "Parameter FarHome: missing directory '$FarHome'." }

# ensure cache
$FarPackages = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarPackages)
if (![System.IO.Directory]::Exists($FarPackages)) { $null = mkdir $FarPackages }

### Download
$web = New-Object -TypeName System.Net.WebClient
$web.UseDefaultCredentials = $true
if ($PackagePath) {
	$PackagePath = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($PackagePath)
	if (![IO.File]::Exists($PackagePath)) { Write-Error "Missing file '$PackagePath'." }
}
else {
	if (!$PackageId) { Write-Error "Parameter PackageId or PackagePath must be specified." }

	# get version
	if (!$PackageVersion) {
		Write-Host -ForegroundColor Cyan "Getting the latest version..."
		$Uri = "http://www.nuget.org/api/v2/Packages()?`$filter=Id eq '$PackageId' and IsLatestVersion eq true"
		$xml = [xml]$web.DownloadString($Uri)
		foreach($_ in $xml.feed.entry) {
			$PackageVersion = $_.properties.Version
			break
		}
		if (!$PackageVersion) { Write-Error "Cannot get the latest version for '$PackageId'." }
		Write-Host -ForegroundColor Cyan "The latest version is '$PackageVersion'."
	}

	# exists?
	$PackagePath = "$FarPackages\$PackageId.$PackageVersion.nupkg"
	if ([System.IO.File]::Exists($PackagePath)) {
		Write-Host -ForegroundColor Cyan "Skipping existing '$PackagePath'. Use PackagePath to update from it."
		return
	}

	# download
	$Uri = "http://nuget.org/api/v2/package/$PackageId/$PackageVersion"
	Write-Host -ForegroundColor Cyan "Downloading '$PackagePath'..."
	$web.DownloadFile($Uri, $PackagePath)
}

function EnsurePlatform {
	if (!$Platform) {
		if (!($exe = Get-Item -LiteralPath "$FarHome\Far.exe" -ErrorAction 0) -or ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b')) {
			Write-Error "Cannot get info from Far.exe. Specify parameter Platform."
		}
		$Platform = $matches[1]
	}
}

function Install($from, $to) {
	$temp = "$env:TEMP\$([guid]::NewGuid())"
	$null = mkdir $temp
	try {
		# unzip
		$shell.Namespace($temp).CopyHere($from.items())

		# copy unescaped files
		foreach($path in Get-ChildItem -LiteralPath $temp -Force -Recurse -Name) {
			# skip folder, unescape file
			if ([System.IO.Directory]::Exists("$temp\$path")) {continue}
			$path2 = [System.Uri]::UnescapeDataString($path)

			# ensure destination folder
			$dir2 = "$to\$([System.IO.Path]::GetDirectoryName($path2))"
			if (![System.IO.Directory]::Exists($dir2)) { $null = mkdir $dir2 }

			# copy original to unescaped
			Copy-Item -LiteralPath "$temp\$path" -Destination "$to\$path2" -Force
		}
	}
	finally {
		[System.IO.Directory]::Delete($temp, $true)
	}
}

### Extract
Write-Host -ForegroundColor Cyan "Extracting from '$PackagePath'..."

# copy as zip for shell
$zip = "$env:TEMP\" + [IO.Path]::GetFileName($PackagePath) + '.zip'
Copy-Item -LiteralPath $PackagePath -Destination $zip

# shell
$shell = New-Object -ComObject Shell.Application

# FarHome.x64
$from = $shell.Namespace("$zip\tools\FarHome.x64")
if ($from) {
	EnsurePlatform
	if ($Platform -eq 'x64') { Install $from $FarHome }
}

# FarHome.x86
$from = $shell.Namespace("$zip\tools\FarHome.x86")
if ($from) {
	EnsurePlatform
	if ($Platform -ne 'x64') { Install $from $FarHome }
}

# FarHome
$from = $shell.Namespace("$zip\tools\FarHome")
if ($from) { Install $from $FarHome }

# About
$from = $shell.Namespace("$zip\tools\About")
if ($from) {
	$to = "$FarPackages\$([System.IO.Path]::GetFileNameWithoutExtension($PackagePath))"
	Install $from $to
}

### end
Remove-Item -ErrorAction 0 -LiteralPath $zip
Write-Host -ForegroundColor Green "Update succeeded."
