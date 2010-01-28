
<#
.SYNOPSIS
	Sets script file descriptions to their help synopses.
	Author: Roman Kuzmin

.DESCRIPTION
	For any *.ps1 script in the directory it takes the first line of the
	synopsis, if any, and sets it as the file description (Descript.ion).

	This is an example of how PowerShell extended type system works.
	PowerShellFar adds the property FarDescription to FileSystemInfo objects:
	FileInfo, DirectoryInfo and related PowerShell FileSystem provider items.

	WARNING: if you are going to use this script then consider to edit script
	synopses, not descriptions, otherwise your description changes may be lost.
#>

param
(
	# Directory path where descriptions of *.ps1 scripts are updated.
	$DirectoryPath = $(throw)
)

Import-Module FarDescription

[System.IO.Directory]::GetFiles($DirectoryPath, '*.ps1') | .{process{
	try {
		$path = $_
		$help = Get-Help $path -ErrorAction Stop
		$file = [System.IO.FileInfo]$path
		$synopsis = $help.Synopsis.Trim()
		if ($synopsis -notlike "$($file.Name)*") {
			$description = ($synopsis -split '[\r\n]+')[0]
			if ($file.FarDescription -cne $description) {
				$file.FarDescription = $description
			}
		}
	}
	catch {
		Write-Error "Cannot get help for '$path': $_"
	}
}}
