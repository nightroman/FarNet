
<#
.SYNOPSIS
	Removes empty strings from a list.
	Author: Roman Kuzmin

.DESCRIPTION
	An item is treated as empty if its string representation is empty or
	contains white spaces only.

.EXAMPLE
	# Remove all empty lines from editor selection:
	Remove-EmptyString- $Far.Editor.SelectedLines

.EXAMPLE
	# Remove double empty lines from editor text:
	Remove-EmptyString- $Far.Editor.Lines 2
#>

param
(
	# Input list of any objects.
	$List
	,
	# Empty line count: 1: any line, 2: double lines and etc.
	$Count = 1
)

$found = 1
for($i = $List.Count; --$i -ge 0;) {
	if ($List[$i] -match '^\s*$') {
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
