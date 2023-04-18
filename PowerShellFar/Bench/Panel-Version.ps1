<#
.Synopsis
	Shows file version information.
	Author: Roman Kuzmin

.Description
	This command opens version information panel for the specified file.

.Parameter Path
		Specifies the file path.
		Default: The current panel file.

.Example
	> Panel-Version.ps1
	Show the current file version.

.Example
	> Panel-Version.ps1 $env:FarHome\Far.exe
	Show the specified file version.
#>

[CmdletBinding()]
param(
	[string]$Path
)

trap {Write-Error $_}
$ErrorActionPreference = 1
if ($Host.Name -ne 'FarHost') {throw 'Please run in FarHost.'}

if (!$Path) {
	$Path = Get-FarPath
}

$item = Get-Item -LiteralPath $Path -Force
$version = try {$item.VersionInfo} catch {}
if ($version) {
	#! FileVersion may be null, e.g. some dotnet made assembly
	if ($version.FileVersion) {
		Open-FarPanel $version -Title "$($item.PSChildName) VersionInfo"
		return
	}
	elseif ($item.Extension -match '^\.(dll|exe)$') {
		try {
			$version = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($item.FullName).GetName().Version
			$Far.UI.WriteLine("$($item.Name) $version")
		}
		catch {
			$Far.UI.WriteLine("$($item.Name) has no known version info.")
		}
		return
	}
}

Show-FarMessage "Cannot get VersionInfo for '$Path'." Panel-Version.ps1
