<#
.Synopsis
	Selects panel files by the specified filter.
	Author: Roman Kuzmin

.Description
	The script uses the scriptblock filter -Where to collect the files from the
	active or passive panel and selects them. Filter operates on FarNet.FarFile
	represented by $_ and returns $true or $false. Basically it works similar
	to Find-FarFile -Where but all found files get selected.

.Parameter Where
		Selection filter: $_ is [FarNet.FarFile] item.

.Parameter Passive
		Tells to work on the passive panel.

.Link
	Find-FarFile

.Example
	# Select by names *\bin\*, e.g. in the the temp panel search results:
	Select-FarFile- { $_.Name -like '*\bin\*' }

	# Select by descriptions starting with TODO or DONE:
	Select-FarFile- { $_.Description -match '^TODO|^DONE' }
#>

param(
	[scriptblock]$Where = {$true}
	,
	[switch]$Passive
)

# target panel: active or passive
$panel = if ($Passive) { $Far.Panel2 } else { $Far.Panel }

### collect indexes to select at
$private:index = -1
$indexes = foreach($_ in $panel.Files) {
	++$index
	if (& $Where) {
		$index
	}
}

### select at indexes and redraw
$panel.SelectAt($indexes)
$panel.Redraw()
