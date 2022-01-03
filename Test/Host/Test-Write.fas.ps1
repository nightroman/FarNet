<#
.Synopsis
	Test writing and Unicode issues

.Description
	Far 3.0.4146 http://forum.farmanager.com/viewtopic.php?p=124447#p124447
#>

### Used to be CodeToChar fix; CodeToChar was removed
job {
	$key = New-Object FarNet.KeyInfo (8356, 8356, 0, 1)
	$char = $Far.KeyInfoToName($key)
	Assert-Far $char -eq '₤'
}

### Write(string)

# panels up
macro 'Keys"CtrlUp CtrlUp"'

# invoke a command with tiny output with Unicode
macro 'Keys[[Esc p s : " £ " Enter]]'

# copy it
macro 'Keys"AltIns Up Home ShiftRight Enter"'

# panels down
macro 'Keys"CtrlDown CtrlDown"'

job {
	# clip?
	$r = $Far.PasteFromClipboard()
	Assert-Far $r -eq '£'
}
