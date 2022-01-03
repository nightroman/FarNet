# Cover crash on invalid non-modal dialog parameters and leaks after fixing the crash
# https://github.com/FarGroup/FarManager/issues/66

Assert-Far (!$Error) -Message 'Please clear $Errors.'
$err = $null
try {
	#! used to crash on Open for invalid dialog parameters (3, should be 8)
	$dialog = $Far.CreateDialog(1, 6, 52, 3)
	#! used to leak in addition to allocated items
	$text = $dialog.AddText(1, 1, 50, 'leak')
	#! used to crash
	$dialog.Open()
}
catch {
	$err = $_
}
Assert-Far ('Exception calling "Open" with "0" argument(s): "Cannot create dialog."' -eq $err)
$Error.Clear()
