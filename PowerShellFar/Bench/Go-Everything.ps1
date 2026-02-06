<#
.Synopsis
	Goes to file system items found by Everything.
	Author: Roman Kuzmin

.Description
	It calls Search-Everything and shows the list of results.
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

		If Filter contains "edit:" or ends with ":edit" (or just ":") then
		this string is removed, "file:" is used instead and the found file
		is opened in editor.

		Relaxed space: allow spaces after colons ":" used in filter.

		Backtick rule: allow two backticks, they are removed together with text
		after the second. This is suitable for copying from markdown notes, see
		example below.

.Parameter Limit
		Specifies the maximum number of results.
		Default: Settings/Limit

.Parameter Here
		Tells to search in the current location.

.Parameter All
		Tells to ignore Settings/PathExclude.

.Example
	>
	# Using Lua macro command prefix "see:"

		CommandLine {
		  prefixes = "see";
		  description = "Go-Everything.ps1";
		  action = function(prefix, text)
		    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:Go-Everything.ps1 '" .. text:gsub("'", "''") .. "'")
		  end;
		}

	you can make simple calls:

		see: <filter>

	Example with allowed spaces and backticks:

		see: folder: `<part-to-take>` <part-to-drop>
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
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

# preprocess filter, undo relaxed spaces and backticks enclosing a part
$Filter = $Filter.Trim() -replace ':\s+', ':' -replace '`(.*?)`.*', '$1'

### Make settings
$settings = [FarNet.User]::GetOrAdd('GoEverything', {
	Add-Type @'
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

### Edit settings
if (!$Filter) {
	return $settings.Edit()
}

### Edit?
$isEdit = $false
if ($Filter -match '(^.*):(?:edit|edi|ed|e)?\s*$') {
	$isEdit = $true
	$Filter = 'file:' + $Matches[1]
}
elseif ($Filter -match '\bedit:') {
	$isEdit = $true
	$Filter = $Filter.Replace('edit:', 'file:')
}

$data = $settings.GetData()

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

if ($isEdit) {
	Open-FarEditor $Path
	return
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
