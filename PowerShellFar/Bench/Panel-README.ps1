<#
.Synopsis
	Shows folder tree for exploring README.md files.
	Author: Roman Kuzmin

.Description
	Requires:
	- Show-FarMarkdown.ps1 for opening in the browser

	Folders with README without headings: "{name} # README".
	Folders with README with headings: "{name} #{heading}".
	Folders without README: "{name}".

	Keys and actions:
	[Enter]
		Close the panel and open the cursor folder.
	[ShiftEnter]
		Open the cursor folder in the passive panel.
	[F4]
		Open the cursor folder README.md in the editor.
	[F3]
		Open the cursor folder README.md in the browser.

.Parameter Root
		Specifies the root directory path.
		Default: the current location.
#>

[CmdletBinding()]
param(
	[string]$Root
)

function global:Panel-Readme-FileName($Root) {
	$name = [System.IO.Path]::GetFileName($Root)

	$markdown = "$Root\README.md"
	if (!([System.IO.File]::Exists($markdown))) {
		return $name
	}

	foreach($line in [System.IO.File]::ReadAllLines($markdown)) {
		if ($line -match '^(#{1,6}\s+.+)$') {
			return $name + ' ' + ($Matches[1] -replace '[\\/]', '|')
		}
	}

	$name + ' # README'
}

#requires -Version 7.4
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$Root = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Root)

### explorer
$explorer = [PowerShellFar.TreeExplorer]::new()

### root file
$file = $explorer.RootFiles.Add()
$file.Name = Panel-Readme-FileName $Root
$file.Data = @{
	Path = "$Root"
}
$file.Fill = {
	param($1)
	$Root = $1.Data.Path
	foreach($dirName in Get-ChildItem -LiteralPath $Root -Name -Directory) {
		if ($dirName.StartsWith('.') -or [System.IO.Path]::GetFileName($dirName) -in @('bin', 'obj', 'debug', 'release')) {
			continue
		}
		$dirPath = "$Root\$dirName"

		$file = $1.ChildFiles.Add()
		$file.Fill = $1.Fill
		$file.Name = Panel-Readme-FileName $dirPath
		$file.Data = @{
			Path = $dirPath
		}
	}
}
$file.Expand()

### panel
$panel = [PowerShellFar.TreePanel]::new($explorer)
$panel.Title = $Root

### .AsOpenFile
$panel.AsOpenFile = {
	param($1, $2)
	$1.Close()
	$Far.Panel.CurrentDirectory = $2.File.Data.Path
}

### .AsEditFile
$panel.AsEditFile = {
	param($1, $2)
	$markdown = "$($2.Data.Path)\README.md"
	if (Test-Path -LiteralPath $markdown) {
		Open-FarEditor "$($2.Data.Path)\README.md"
	}
}

### .AsViewFile
$panel.AsViewFile = {
	param($1, $2)
	$markdown = "$($2.Data.Path)\README.md"
	if (Test-Path -LiteralPath $markdown) {
		Show-FarMarkdown.ps1 $markdown
	}
}

### Keys
$panel.add_KeyPressed({
	### [ShiftEnter]
	if ($_.Key.IsShift([FarNet.KeyCode]::Enter)) {
		if ($file = $this.CurrentFile) {
			$Far.Panel2.CurrentDirectory = $file.Data.Path
		}
		return
	}
})

### show
$panel.Open()
