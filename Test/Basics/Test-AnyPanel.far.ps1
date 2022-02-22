
# the panel
$Panel = $Far.Panel

function TestPanelFlag($Flag)
{
	# the original mode, to be restored
	$1 = $Panel.$Flag

	# test new mode
	$Panel.$Flag = !$1
	Assert-Far ($Panel.$Flag -ne $1) -Message "Flag = $Flag"

	# test/restore old mode
	$Panel.$Flag = $1
	Assert-Far ($Panel.$Flag -eq $1) -Message "Flag = $Flag"
}

### test settable panel flags
TestPanelFlag DirectoriesFirst

### test sort mode Unsorted, then set Name
$Panel.SortMode = 'UnsortedReversed'
Assert-Far ($Panel.SortMode -eq 'UnsortedReversed')
$Panel.SortMode = 'Name'
Assert-Far ($Panel.SortMode -eq 'Name')
