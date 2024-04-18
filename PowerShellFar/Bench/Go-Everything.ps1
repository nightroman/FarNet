<#
.Synopsis
	Goes to file system items found by Everything.
	Author: Roman Kuzmin

.Description
	This command calls Search-Everything and shows the list of results.
	Select an item in order to navigate to it in the panel.
	Select the item <Panel> in order to panel the results.

	Requires:
	- Everything running, https://www.voidtools.com
	- PSEverything installed, https://www.powershellgallery.com/packages/PSEverything

	Use $env:EVERYTHING_PATH_EXCLUDE to define paths to exclude.
	Use the switch All to ignore this variable if it is defined.

.Parameter Filter
		Specifies the search filter used by Everything.
		See: help Search-Everything -Parameter Filter

.Parameter Limit
		Specifies the maximum number of results.
		Default: 9999

.Parameter Here
		Tells to search in the current location.

.Parameter All
		Tells to ignore exclude paths defined by $env:EVERYTHING_PATH_EXCLUDE.
#>

param(
	[Parameter(Mandatory=1)]
	[string]$Filter
	,
	[int]$Limit = 9999
	,
	[switch]$Here
	,
	[switch]$All
)

#requires -Version 7.4 -Modules PSEverything
$ErrorActionPreference = 1
trap { $PSCmdlet.ThrowTerminatingError($_) }
if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

### Get items

$Filter = $Filter -replace '(?<=\w+:)\s+'

if ($All) {
	$PathExclude = $null
}
else {
	$PathExclude = $env:EVERYTHING_PATH_EXCLUDE
	if ($PathExclude) {
		$PathExclude = $PathExclude -split ';'
	}
}

$Items = Search-Everything -Filter $Filter -PathExclude $PathExclude -Global:(!$Here) | Select-Object -First $Limit
if (!$Items) {
	return
}

### Sort items

function Get-MatchLength($1, $2) {
	$i = 0
	while ($i -lt $1.Length -and $i -lt $2.Length -and $1[$i] -eq $2[$i]) {
		++$i
	}
	$i
}

$root = "$PWD\"
$Items = @($Items | Sort-Object {- (Get-MatchLength $root $_)}, {$_})

### Select

if ($Items.Count -eq 1) {
	$Path = $Items[0]
}
else {
	$Path = @($Items; '<Panel>') | Out-FarList -Title $Filter
	if (!$Path) {
		return
	}

	if ($Path -eq '<Panel>') {
		$Items | Out-FarPanel -Title Everything -Columns @{n=$Filter; e={$_}}
		return
	}
}

if ($Far.Panel.IsPlugin) {
	$Far.Panel.Close()
	if ($Far.Panel.IsPlugin) {
		return
	}
}

### Navigate

if ([System.IO.File]::Exists($Path)) {
	$Far.Panel.GoToPath($Path)
}
else {
	$Far.Panel.CurrentDirectory = $Path
}
