# Stop and Free were not called due to exception in Closing.
# _201118_vk After fixing it crashed at first.

job {
	Assert-Far -Title Ensure -NoError

	$dialog = $Far.CreateDialog(-1, -1, 52, 3)
	$t = $dialog.AddText(1, 1, 50, '_201118_vk')
	$dialog.add_Closing({
		throw 42
	})
	$dialog.Open()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq '_201118_vk'
}
keys Esc
job {
	Assert-Far -Dialog
	Assert-Far @(
		$__[0].Text -eq 'Error in FarNet::FarDialog::DialogProc'
		$__[1].Text -eq '42'
		$global:Error.Count -ne 0
	)
	$global:Error.Clear()
}
keys Esc
