
<#
	NuGet package tools for Far Manager.
	Author: Roman Kuzmin
#>

<#
.Synopsis
	Installs or updates the specified package from web.
.Description
	This command installs or updates the package from web.
	Packages are downloaded to %LOCALAPPDATA%\NuGet\Cache
.Parameter Id
		The package ID, e.g. FarNet.PowerShellFar
.Parameter FarHome
		The Far Manager directory.
		By default the current location is used.
.Parameter Platform
		Far Manager platform, x64 or x86|Win32.
		It is not needed if Far.exe is in FarHome.
.Parameter Version
		The package version, e.g. 5.0.40
		By default the latest is assumed.
.Parameter Source
		The package web source.
		By default the NuGet gallery is used.
#>
function Install-FarPackage(
	[Parameter(Position=0, Mandatory=1)]
	[string]
	$Id,
	[string]
	[Parameter(Position=1)]
	$FarHome = '.',
	[string]
	[ValidateSet('x64', 'x86', 'Win32', '')]
	$Platform,
	[string]
	$Version,
	[string]
	$Source = 'https://www.nuget.org/api/v2'
)
{try{
	# web client
	$web = New-Object -TypeName System.Net.WebClient
	$web.UseDefaultCredentials = $true

	# get latest version
	if (!$Version -or $Version[0] -eq '?') {
		Write-Host "Getting the latest version of '$Id'"
		$xml = [xml]$web.DownloadString("$Source/Packages()?`$filter=Id eq '$Id' and IsLatestVersion eq true")
		$latest = try {
			foreach($_ in $xml.feed.entry) {
				if ($_.id -match "Id='([^']+)'") { $Id = $Matches[1] }
				$_.properties.Version
				break
			}
		} catch {}

		if (!$latest) {throw "Cannot get the latest version of '$Id'."}
		Write-Host "The latest version is '$latest'."

		# return or skip latest
		if ($Version -match '^\?(.*)') {
			if (!$Matches[1]) {
				return $latest
			}
			if ($Matches[1] -eq $latest) {
				Write-Host "'$Id' is up to date." -ForegroundColor Cyan
				return
			}
		}

		# set latest
		$Version = $latest
	}

	# download?
	$CacheDirectory = "$env:LOCALAPPDATA\NuGet\Cache"
	$Path = "$CacheDirectory\$Id.$Version.nupkg"
	if ([System.IO.File]::Exists($Path)) {
		# use nupkg
		Write-Host "Found package '$Path'."
	}
	else {
		# download nupkg
		Write-Host "Downloading package '$Path'" -ForegroundColor Cyan
		$null = [System.IO.Directory]::CreateDirectory($CacheDirectory)
		$web.DownloadFile("$Source/package/$Id/$Version", $Path)
	}

	# unpack
	Restore-FarPackage -Path:$Path -FarHome:$FarHome -Platform:$Platform -Source:$Source
}catch{$PSCmdlet.ThrowTerminatingError($_)}}

<#
.Synopsis
	Installs a package from the specified package file.
.Description
	This command can be used where internet is missing.
.Parameter Path
		The package file.
.Parameter FarHome
		The Far Manager directory.
		By default the current location is used.
.Parameter Platform
		Far Manager platform, x64 or x86|Win32.
		It is not needed if Far.exe is in FarHome.
.Parameter Source
		The package web source.
		Normally it should be omitted.
#>
function Restore-FarPackage(
	[Parameter(Position=0, Mandatory=1)]
	[string]
	$Path,
	[Parameter(Position=1)]
	[string]
	$FarHome = '.',
	[string]
	[ValidateSet('x64', 'x86', 'Win32', '')]
	$Platform,
	[string]
	$Source
)
{try{
	$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)
	if (![System.IO.File]::Exists($Path)) {throw "Missing package '$Path'."}

	$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
	$exe = [System.IO.FileInfo]"$FarHome\Far.exe"
	if ($exe.Exists) {
		foreach($_ in Get-Process [F]ar) {
			if ($_.Path -eq $exe.FullName) {throw "Close Far Manager and repeat."}
		}
		if ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b') {
			throw "Cannot get info from 'Far.exe'."
		}
		$Platform = $Matches[1]
	}

	Add-Type -AssemblyName WindowsBase
	$package = [System.IO.Packaging.Package]::Open($Path, 'Open', 'Read')
	try {
		# Id and Version from package
		$Id = $package.PackageProperties.Identifier
		$Version = $package.PackageProperties.Version
		if (!$Id -or !$Version) {throw "Invalid package '$Path'."}

		# collect parts and check x86, x64
		if ($Platform -eq 'Win32') {$Platform = 'x86'}
		$parts = foreach($part in $package.GetParts()) {
			if ($part.Uri -notmatch '^/tools/FarHome\.?(x..)?/') {continue}
			if ($Matches[1]) {
				if (!$Platform) {throw "Please, specify the Platform."}
				if ($Matches[1] -ne $Platform) {continue}
			}
			$part
		}

		# old info, get Source, uninstall
		$info = "$FarHome\Update.$Id.info"
		if ([System.IO.File]::Exists($info)) {
			if (!$Source) {
				$Source, $null = [System.IO.File]::ReadAllLines($info)
			}
			Uninstall-FarPackage -Id:$Id -FarHome:$FarHome
		}
		else {
			$null = [System.IO.Directory]::CreateDirectory($FarHome)
		}

		# new info
		Write-Host "Installing '$Id' in '$FarHome'" -ForegroundColor Cyan
		[System.IO.File]::WriteAllText($info, "$Source`r`n$Version`r`n")

		# unpack, install
		$CLR4 = $PSVersionTable.CLRVersion.Major -ge 4
		foreach($part in $parts) {
			if ($part.Uri -notmatch '^/tools/FarHome[^/]*/(.*)') {continue}
			$it = [System.Uri]::UnescapeDataString($Matches[1])
			$to = "$FarHome/$it"

			$null = [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($to))
			$stream2 = New-Object System.IO.FileStream $to, Create
			try {
				[System.IO.File]::AppendAllText($info, "$it`r`n")
				$stream1 = $part.GetStream('Open', 'Read')
				if ($CLR4) {
					$stream1.CopyTo($stream2)
				}
				else {
					$buffer = New-Object byte[] ($n = $stream1.Length)
					$null = $stream1.Read($buffer, 0, $n)
					$stream2.Write($buffer, 0, $n)
				}
			}
			finally {
				$stream2.Close()
			}
		}
	}
	finally {
		$package.Close()
	}
}catch{$PSCmdlet.ThrowTerminatingError($_)}}

<#
.Synopsis
	Updates installed packages if newer versions exist.
.Description
	This command checks and updates all installed packages.
	To update a single package alone use Install-FarPackage.
.Parameter FarHome
		The Far Manager directory.
		By default the current location is used.
.Parameter Platform
		Far Manager platform, x64 or x86|Win32.
		It is not needed if Far.exe is in FarHome.
#>
function Update-FarPackage(
	[Parameter(Position=0)]
	[string]$FarHome = '.',
	[string]
	[ValidateSet('x64', 'x86', 'Win32', '')]
	$Platform
)
{try{
	$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
	Write-Host "Updating packages in '$FarHome'"
	foreach($info in Get-Item "$FarHome\Update.*.info") {
		if ($info.Name -notmatch '^Update\.(.+)\.info$') {throw}
		$Id = $Matches[1]
		$Source, $Version, $null = [System.IO.File]::ReadAllLines($info.FullName)
		if (!$Source) {
			Write-Warning "Cannot update '$Id'. Missing source in '$info'."
			continue
		}
		Install-FarPackage -Id:$Id -FarHome:$FarHome -Platform:$Platform -Version:"?$Version" -Source:$Source
	}
}catch{$PSCmdlet.ThrowTerminatingError($_)}}

<#
.Synopsis
	Uninstalls the specified package.
.Description
	It removes files listed in "Update.$Id.info" and empty directories.
.Parameter Id
		The package ID.
.Parameter FarHome
		The Far Manager directory.
		By default the current location is used.
#>
function Uninstall-FarPackage(
	[Parameter(Mandatory=1)]
	[string]$Id,
	[string]$FarHome = '.'
)
{try{
	$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
	Write-Host "Uninstalling '$Id' in '$FarHome'" -ForegroundColor Cyan

	$info = "$FarHome\Update.$Id.info"
	if (![System.IO.File]::Exists($info)) {throw "Missing required file '$info'."}

	# removes empty parent
	function Remove-FarPackageEmpty($item) {
		if (($dir = [System.IO.Path]::GetDirectoryName($item)) -eq $FarHome -or [System.IO.Directory]::GetFileSystemEntries($dir)) {
			return
		}
		Write-Host "Removing empty directory '$dir'"
		try {
			[System.IO.Directory]::Delete($dir)
			Remove-FarPackageEmpty $dir
		}
		catch {
			Write-Warning "Cannot remove '$dir'."
		}
	}

	$null, $null, $lines = [System.IO.File]::ReadAllLines($info)
	foreach($it in $lines) {
		$to = "$FarHome\$it"
		[System.IO.File]::Delete($to)
		Remove-FarPackageEmpty $to
	}
	[System.IO.File]::Delete($info)
}catch{$PSCmdlet.ThrowTerminatingError($_)}}
