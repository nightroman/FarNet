
<#
.Synopsis
	Gets, unpacks, updates, or removes Far Manager NuGet packages.
	Author: Roman Kuzmin

.Description
	Recommendations, not always requirements:
	- Close all running Far Manager instances before updating.
	- Invoke the script by the console host, i.e. PowerShell.exe.

	On updating the script downloads, unpacks, and installs package files. In
	addition it installs to FarHome files "Update.<Id>.info". These files are
	used on automatic updates and removals. Remove them if this is not needed
	but do not modify or rename.

	Use the switch Verbose in order to get verbose messages. Note that verbose
	messages are enabled internally on automatic updates and removals.

	If an update or removal fails due to temporary problems like locked files
	then repeat the same operation after resolving issues as soon as possible.

.Parameter Id
		The package ID, e.g. 'FarNet', 'FarNet.PowerShellFar'. It is specified
		for updating or removing of a single package.

		If it is omitted then automatic update is performed. Each package in
		FarHome with its "Update.<Id>.info" is checked for a newer latest
		version. If it is available then it is downloaded, old files are
		automatically removed, and new files are installed.
.Parameter Version
		The package version, e.g. '5.0.40'. It is specified for installing a
		particular version. If it is omitted then the latest version is taken
		from NuGet. If it is "?" then the script returns the latest and stops.
.Parameter Source
		Specifies the package Web source.
		Default: https://www.nuget.org/api/v2
.Parameter CacheDirectory
		The directory for downloaded package files (<Id>.<Version>.nupkg).
		The script downloads only missing packages.
		Default: "$env:LOCALAPPDATA\NuGet\Cache".
.Parameter Remove
		Tells to remove installed files and empty directories. The package is
		specified by Id. Its installed files are removed, added are not. This
		operation uses "Update.<Id>.info" in FarHome and removes it, too.
.Parameter OutputDirectory
		The destination of unpacked directories. Default: the current location.
		The unpacked directories names are "<Id>.<Version>".
.Parameter FarHome
		The Far Manager directory to be updated. On automatic updates/removals
		the current location is used by default. On other updates the script
		only downloads and unpacks if FarHome is omitted.
.Parameter Platform
		Platform: x64 or x86|Win32. The default is extracted from Far.exe. It
		is needed only for packages with FarHome.x64|x86 folders if Far.exe is
		not in FarHome or its info cannot be extracted.
.Parameter Path
		Specifies the path to existing package file. Id and Version are taken
		from it. This kind of update is not recommended because the Source is
		unknown and automatic updates are not be possible until the package is
		updated from Web as usual.

.Example
	> Update-FarPackage [-FarHome <path>]

	Updates installed packages if newer versions are available.
	Note that only packages with "Update.<Id>.info" are processed.

.Example
	> Update-FarPackage FarNet ?

	This command just requests and returns the latest FarNet version string.

.Example
	> Update-FarPackage FarNet -Verbose

	This command downloads the latest FarNet and unpacks it to the current
	directory as FarNet.<Version>. It does not install any files because
	FarHome is omitted. Verbose messages are enabled.

.Example
	> Update-FarPackage FarNet -FarHome <path> [-Platform <x64|x86>]

	This command updates FarNet in FarHome. The Platform is needed if Far.exe
	is not there (normally it is). After updating look at extra files at the
	output directory FarNet.<Version>, e.g. About-FarNet.htm, FarNetAPI.chm,
	History.txt.

.Example
	> Update-FarPackage FarNet.PowerShellFar -FarHome <path>

	This command updates PowerShellFar. The Platform is not needed. After
	updating look at extra files at FarNet.PowerShellFar.<Version>, e.g.
	About-PowerShellFar.htm, History.txt.

.Inputs
	None
.Outputs
	None on operations. The latest version on a version request.

.Link
	https://farnet.googlecode.com/svn/trunk/PowerShellFar/Bench/Update-FarPackage.ps1
#>

[CmdletBinding(DefaultParameterSetName='Id')]
param(
	[Parameter(ParameterSetName='Remove', Position=0, Mandatory=1)]
	[Parameter(ParameterSetName='Id', Position=0)]
	[string]
	$Id,
	[Parameter(ParameterSetName='Id', Position=1)]
	[string]
	$Version,
	[Parameter(ParameterSetName='Id')]
	[string]
	$Source = 'https://www.nuget.org/api/v2',
	[Parameter(ParameterSetName='Id')]
	[string]
	$CacheDirectory = "$env:LOCALAPPDATA\NuGet\Cache",
	[Parameter(ParameterSetName='Path', Mandatory=1)]
	[string]
	$Path,
	[Parameter(ParameterSetName='Id')]
	[Parameter(ParameterSetName='Path')]
	[string]
	$OutputDirectory = '.',
	[string]
	$FarHome,
	[Parameter(ParameterSetName='Id')]
	[Parameter(ParameterSetName='Path')]
	[string][ValidateSet('x64', 'x86', 'Win32', '')]
	$Platform,
	[Parameter(ParameterSetName='Remove', Mandatory=1)]
	[switch]
	$Remove
)
try {&{ # new scope and errors

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# removes empty directories
function RemoveDirectory($item) {
	if (($dir = [System.IO.Path]::GetDirectoryName($item)) -eq $FarHome -or [System.IO.Directory]::GetFileSystemEntries($dir)) {
		return
	}
	Write-Verbose "Removing empty directory '$dir'..."
	try {
		[System.IO.Directory]::Delete($dir)
		RemoveDirectory $dir
	}
	catch {
		Write-Warning "Cannot remove '$dir'."
	}
}

### remove the package
if ($PSCmdlet.ParameterSetName -eq 'Remove') {
	$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
	$info = "$FarHome\Update.$Id.info"
	if (![System.IO.File]::Exists($info)) { throw "Missing '$info' required for package removal." }
	$null, $null, $lines = [System.IO.File]::ReadAllLines($info)
	foreach($name in $lines) {
		$to = "$FarHome\$name"
		if ([System.IO.File]::Exists($to)) {
			[System.IO.File]::Delete($to)
			RemoveDirectory $to
		}
	}
	[System.IO.File]::Delete($info)
	return
}

### use or download package
if ($PSCmdlet.ParameterSetName -eq 'Path') {
	$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)
	if (![System.IO.File]::Exists($Path)) { throw "Missing '$Path'. Check the package Path." }
	$Source = ''
}
else {
	### update all installed
	if (!$Id) {
		Write-Verbose -Verbose "Automatic update of installed packages."
		$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)
		foreach($info in Get-Item "$FarHome\Update.*.info") {
			if ($info.Name -notmatch '^Update\.(.+)\.info$') {continue}
			$Id = $Matches[1]
			$Source, $Version, $null = [System.IO.File]::ReadAllLines($info.FullName)
			if (!$Source) {
				Write-Warning "Cannot update '$Id' automatically."
				continue
			}
			& $MyInvocation.ScriptName -Id:$Id -Version:"?$Version" -Source:$Source -Verbose `
			-FarHome:$FarHome -Platform:$Platform -CacheDirectory:$CacheDirectory -OutputDirectory:$OutputDirectory
		}
		return
	}

	# web client
	$web = New-Object -TypeName System.Net.WebClient
	$web.UseDefaultCredentials = $true

	### get latest version
	if (!$Version -or $Version[0] -eq '?') {
		Write-Verbose "Getting the latest version of '$Id'..."
		$xml = [xml]$web.DownloadString("$Source/Packages()?`$filter=Id eq '$Id' and IsLatestVersion eq true")
		$latest = try {
			foreach($_ in $xml.feed.entry) {
				if ($_.id -match "Id='([^']+)'") { $Id = $Matches[1] }
				$_.properties.Version
				break
			}
		} catch {}

		if (!$latest) { throw "Cannot get the latest version of '$Id'. Check the package ID." }
		Write-Verbose "The latest version is '$latest'."

		### return or skip latest
		if ($Version -match '^\?(.*)') {
			if (!$Matches[1]) {
				return $latest
			}
			if ($Matches[1] -eq $latest) {
				Write-Verbose "The latest version is installed."
				return
			}
		}

		# set latest
		$Version = $latest
	}

	# nupkg exists?
	$CacheDirectory = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($CacheDirectory)
	$Path = "$CacheDirectory\$Id.$Version.nupkg"
	if ([System.IO.File]::Exists($Path)) {
		# use nupkg
		Write-Verbose "Found package '$Path'."
	}
	else {
		### download nupkg
		Write-Verbose "Downloading package '$Path'..."
		$null = [System.IO.Directory]::CreateDirectory($CacheDirectory)
		$web.DownloadFile("$Source/package/$Id/$Version", $Path)
	}
}

### unpack to output
Add-Type -AssemblyName WindowsBase
$package = [System.IO.Packaging.Package]::Open($Path, 'Open', 'Read')
try {
	# get "actual" Id and Version
	$Id = $package.PackageProperties.Identifier
	$Version = $package.PackageProperties.Version
	if (!$Id -or !$Version) { throw "Invalid package '$Path'." }

	# output directory
	$output = Join-Path $PSCmdlet.GetUnresolvedProviderPathFromPSPath($OutputDirectory) "$Id.$Version"
	Remove-Item -LiteralPath $output -Force -Recurse -ErrorAction 0
	Write-Verbose "Unpacking to '$output'..."

	$CLR3 = $PSVersionTable.CLRVersion.Major -le 3
	foreach($part in $package.GetParts()) {
		if ($part.Uri -notmatch '^/tools/(.*)') {continue}
		$to = "$output/$([System.Uri]::UnescapeDataString($Matches[1]))"
		$null = [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($to))
		$stream2 = New-Object System.IO.FileStream $to, 'Create'
		try {
			$stream1 = $part.GetStream('Open', 'Read')
			if ($CLR3) {
				$buffer = New-Object byte[] ($n = $stream1.Length)
				$null = $stream1.Read($buffer, 0, $n)
				$stream2.Write($buffer, 0, $n)
			}
			else {
				$stream1.CopyTo($stream2)
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

### FarHome? return?
if (!$FarHome) {return}
$FarHome = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FarHome)

### remove installed
$info = "$FarHome\Update.$Id.info"
if ([System.IO.File]::Exists($info)) {
	Write-Verbose "Removing installed '$Id'..."
	& $MyInvocation.ScriptName -Remove -Id:$Id -FarHome:$FarHome
}

# updates FarHome
function UpdateFarHome($from) {
	foreach($name in Get-ChildItem -Name -LiteralPath $from -Force -Recurse) {
		if ([System.IO.Directory]::Exists("$from\$name")) {continue}
		$null = [System.IO.Directory]::CreateDirectory("$FarHome\$([System.IO.Path]::GetDirectoryName($name))")
		Copy-Item -LiteralPath "$from\$name" -Destination "$FarHome\$name" -Force
		[System.IO.File]::AppendAllText($info, "$name`r`n")
	}
}

# gets platform
function GetPlatform {
	if ($Platform) { return $Platform }
	if (!($exe = Get-Item -LiteralPath "$FarHome\Far.exe" -ErrorAction 0) -or ($exe.VersionInfo.FileVersion -notmatch '\b(x86|x64)\b')) {
		throw "Cannot get info from Far.exe. Specify the Platform."
	}
	($script:Platform = $Matches[1])
}

### update from output
Write-Verbose "Updating '$Id' in '$FarHome'..."
[System.IO.File]::WriteAllText($info, "$Source`r`n$Version`r`n")
try {
	# FarHome.x64
	if ([System.IO.Directory]::Exists(($from = "$output\FarHome.x64"))) {
		if ((GetPlatform) -eq 'x64') {
			UpdateFarHome $from
		}
	}

	# FarHome.x86
	if ([System.IO.Directory]::Exists(($from = "$output\FarHome.x86"))) {
		if ((GetPlatform) -ne 'x64') {
			UpdateFarHome $from
		}
	}

	# FarHome
	if ([System.IO.Directory]::Exists(($from = "$output\FarHome"))) {
		UpdateFarHome $from
	}
}
catch {
	[System.IO.File]::WriteAllText($info, "$Source`r`n<failed>`r`n")
	throw
}

}} catch { Write-Error $_ -ErrorAction Stop }
