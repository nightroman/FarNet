<#
.Synopsis
	Goes to the specified panel item.
	Author: Roman Kuzmin

.Description
	It is similar to Far goto command but the path is any PowerShell path.
	Wildcards are permitted. The selection list is shown for 2+ items.
	A provider panel is opened if the path is not file system.

.Example
	>
	# script or application in the path
	Go-To Far.exe

	# open one of FarNet module folders
	Go-To $env:FARHOME\FarNet\Modules\*\

	# open environment panel
	Go-To env:

	# open registry panel
	Go-To registry::*
	Go-To HKEY_CURRENT_USER\SOFTWARE

.Inputs
	- File system items (e.g. from Get-Item and similar)
	- String paths (e.g from Search-Everything, PSEverything module)

.Parameter Path
		*) Directory or file path. For a directory \ or / at the end tells to
		open the directory in the panel instead of setting the cursor to it.
		Alternatively, use the switch Open.

		If the path looks like just name then applications and scripts in the
		system path are also searched.

		*) Provider path, e.g. registry key path. Only container and drive
		paths are permitted. The script opens the provider path panel.

.Parameter Open
		Tells to open directories instead of setting the cursor.
#>

param(
	[Parameter(ValueFromPipeline=1, ValueFromPipelineByPropertyName=1)]
	[Alias('FullName')]
	[string]$Path
	,
	[switch]$Open
)
begin {
	$Inputs = [System.Collections.Generic.List[string]]::new()
}
process {
	$Inputs.Add($Path)
}
end {
	### Select $Path
	if (!$Inputs) {
		return
	}
	elseif ($Inputs.Count -eq 1) {
		$Path = $Inputs[0]
	}
	else {
		$Path = $Inputs | Out-FarList -Title Go-To
		if (!$Path) {
			return
		}
	}

	### Convert and resolve paths
	if ($Path -like 'HKEY_*') {
		$Path = "Registry::$Path"
	}
	$resolved = @(Resolve-Path $Path -ErrorAction Ignore)

	### Add from system path
	if (!$Path.Contains(':') -and !$Path.Contains('\') -and !$Path.Contains('/')) {
		$resolved += @(
			Get-Command $Path -Type Application -ErrorAction Ignore
			Get-Command $Path -Type ExternalScript -ErrorAction Ignore
		) |
		.{process{ Resolve-Path $_.Path -ErrorAction Ignore }}
	}

	if (!$resolved) {
		return
	}

	### Select the final path
	if ($resolved.Count -eq 1) {
		$xPath = $resolved[0]
	}
	else {
		$xPath = $resolved | Out-FarList -Title Go-To -Text Path
		if (!$xPath) {
			return
		}
	}

	### FileSystem path
	if ($xPath.Provider.Name -eq 'FileSystem') {
		if (($Open -or $Path.EndsWith('\') -or $Path.EndsWith('/')) -and [System.IO.Directory]::Exists($xPath.Path)) {
			$Far.Panel.GoToPath($xPath.Path + '\')
		}
		else {
			$Far.Panel.GoToPath($xPath.Path)
		}
	}
	### Provider path
	else {
		[PowerShellFar.ItemPanel]::new($xPath.Path).Open()
	}
}
