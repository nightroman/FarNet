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

	Without Filter, opens GoEverythingSettings.xml
	- Limit
		The default Limit. Default: 9999
	- PathExclude
		Semicolon separated paths to exclude from search, with environment
		variables expanded. Default: %TEMP%

.Parameter Filter
		Specifies the search filter used by Everything.
		See: help Search-Everything -Parameter Filter

.Parameter Limit
		Specifies the maximum number of results.
		Default: Settings/Limit

.Parameter Here
		Tells to search in the current location.

.Parameter All
		Tells to ignore Settings/PathExclude.
#>

[CmdletBinding()]
param(
	[string]$Filter
	,
	[int]$Limit
	,
	[switch]$Here
	,
	[switch]$All
)

#requires -Version 7.4 -Modules PSEverything
$ErrorActionPreference = 1
trap { $PSCmdlet.ThrowTerminatingError($_) }
if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

### Settings

$sets = [FarNet.User]::GetOrAdd('GoEverything', {
	Add-Type -ReferencedAssemblies System.Xml.ReaderWriter @'
using System.Xml.Serialization;
[XmlRoot("Data")]
public class GoEverything
{
	public int Limit = 9999;
	public string PathExclude = "%TEMP%";

	public static int GetMatchRank(string s1, string s2) {
		int i = 0;
		while (i < s1.Length && i < s2.Length && s1[i] == s2[i]) {
			++i;
		}
		return -i;
	}
}
'@
	[FarNet.ModuleSettings[GoEverything]]::new("$env:FARPROFILE\FarNet\PowerShellFar\GoEverythingSettings.xml")
})

if (!$Filter) {
	return $sets.Edit()
}

$data = $sets.GetData()

### Get items

# set Limit
if (!$Limit) {
	$Limit = $data.Limit
}

# set PathExclude
if ($All) {
	$PathExclude = $null
}
else {
	$PathExclude = $data.PathExclude
	if ($PathExclude) {
		$PathExclude = [System.Environment]::ExpandEnvironmentVariables($PathExclude) -split ';'
	}
}

$Items = Search-Everything -Filter $Filter -PathExclude $PathExclude -Global:(!$Here) | Select-Object -First $Limit
if (!$Items) {
	return
}

### Sort items

$root = $Far.CurrentDirectory + '\'
$Items = @($Items | Sort-Object {[GoEverything]::GetMatchRank($root, $_)}, {$_})

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
