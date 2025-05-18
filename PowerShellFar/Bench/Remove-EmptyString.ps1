<#
.Synopsis
	Removes empty strings from a list.
	Author: Roman Kuzmin

.Description
	An item is treated as empty if its string is empty or white space.

.Parameter List
		Input list of any objects.

		The list type should have:
		- Count, [i], RemoveAt(i)

.Parameter Count
		Specifies consecutive empty line count to be removed:
		1 ~ any empty line, 2 ~ double empty lines, and etc.

.Example
	>
	# Remove all empty lines from editor selection:

	Remove-EmptyString.ps1 $Far.Editor.SelectedLines

.Example
	>
	# Remove double empty lines from editor text:

	Remove-EmptyString.ps1 $Far.Editor.Lines 2
#>

[CmdletBinding()]
param(
	[Parameter(Mandatory=1)]
	$List
	,
	$Count = 1
)

$found = 1
for($i = $List.Count; --$i -ge 0) {
	if ([string]::IsNullOrWhiteSpace($List[$i])) {
		if ($found -ge $Count) {
			$List.RemoveAt($i)
		}
		else {
			++$found
		}
	}
	else {
		$found = 1
	}
}
