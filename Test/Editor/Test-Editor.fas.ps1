<#
.Synopsis
	Editor tests.
#>

### Edit empty text and exit without saving
run {
	# open modal editor with empty text
	$Far.AnyEditor.EditText(@{})
}
job {
	Assert-Far -Editor
}
# exit the editor
keys Esc
job {
	Assert-Far -Panels
}

### Expand tabs and options
run {
	# open modal editor with empty text
	$Far.AnyEditor.EditText(@{})
}
job {
	$Editor = $Far.Editor

	### test ExpandTabs
	$ExpandTabs = $Editor.ExpandTabs
	$Editor.ExpandTabs = [FarNet.ExpandTabsMode]::All
	Assert-Far $Editor.ExpandTabs -eq ([FarNet.ExpandTabsMode]::All)
	$Editor.ExpandTabs = [FarNet.ExpandTabsMode]::New
	Assert-Far $Editor.ExpandTabs -eq ([FarNet.ExpandTabsMode]::New)
	$Editor.ExpandTabs = [FarNet.ExpandTabsMode]::None
	Assert-Far $Editor.ExpandTabs -eq ([FarNet.ExpandTabsMode]::None)
	$Editor.ExpandTabs = $ExpandTabs

	### test IsLocked
	# store
	$IsLocked = $Editor.IsLocked
	# lock
	$Editor.IsLocked = $true
	Assert-Far $Editor.IsLocked
	# unlock
	$Editor.IsLocked = $false
	Assert-Far (!$Editor.IsLocked)
	# restor
	$Editor.IsLocked = $IsLocked

	### test IsVirtualSpace
	$IsVirtualSpace = $Editor.IsVirtualSpace
	$Editor.IsVirtualSpace = $true
	Assert-Far $Editor.IsVirtualSpace
	$Editor.IsVirtualSpace = $false
	Assert-Far (!$Editor.IsVirtualSpace)
	$Editor.IsVirtualSpace = $IsVirtualSpace

	### test ShowWhiteSpace
	$ShowWhiteSpace = $Editor.ShowWhiteSpace
	$Editor.ShowWhiteSpace = $true
	Assert-Far $Editor.ShowWhiteSpace
	$Editor.ShowWhiteSpace = $false
	Assert-Far (!$Editor.ShowWhiteSpace)
	$Editor.ShowWhiteSpace = $ShowWhiteSpace

	### test WriteByteOrderMark
	<#
	$WriteByteOrderMark = $Editor.WriteByteOrderMark
	$Editor.WriteByteOrderMark = $true
	Assert-Far ($Editor.WriteByteOrderMark)
	$Editor.WriteByteOrderMark = $false
	Assert-Far (!$Editor.WriteByteOrderMark)
	$Editor.WriteByteOrderMark = $WriteByteOrderMark
	#>
}

### Test _101210_142325: SetText() drops selection
job {
	# set and select some text
	$Editor = $Far.Editor
	$Editor.SetText("0123456789")
	$Editor[-1].SelectText(2, 7)
	Assert-Far $Editor.SelectionExists

	# set any text -> no selection
	$Editor.SetText("0123456789")
	Assert-Far (!$Editor.SelectionExists)
}

# exit editor
macro 'Keys"Esc n"'
