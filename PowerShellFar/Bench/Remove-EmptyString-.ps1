
<#
.SYNOPSIS
	Removes empty strings from a list
	Author: Roman Kuzmin

.DESCRIPTION
	An item is treated as empty if its string representation is empty or
	contains white spaces only.

.EXAMPLE
	# remove all empty lines from editor selection:
	Remove-EmptyString- $Far.Editor.Selection

.EXAMPLE
	# remove double empty lines from editor text:
	Remove-EmptyString- $Far.Editor.Lines 2

.PARAMETER List
		Input list of any objects.
.PARAMETER Count
		Empty line count: 1: any line, 2: double lines and etc.
#>

param
(
	$List,
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
