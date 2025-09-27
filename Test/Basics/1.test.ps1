
# Cover crash on invalid non-modal dialog parameters and leaks after fixing the crash
# https://github.com/FarGroup/FarManager/issues/66
task 3_0_5261_crash {
	Assert-Far (!$Error) -Message 'Please clear $Errors.'
	try {
		#! used to crash on Open for invalid dialog parameters (3, should be 8)
		$dialog = $Far.CreateDialog(1, 6, 52, 3)
		#! used to leak in addition to allocated items
		$text = $dialog.AddText(1, 1, 50, 'leak')
		#! used to crash
		throw $dialog.Open()
	}
	catch {
		equals "$_" 'Exception calling "Open" with "0" argument(s): "Cannot create dialog."'
		$Error.Clear()
	}
}

task MainAndAsyncCommon {
	& ..\..\Samples\FarTask\MainAndAsyncCommon\Test-CommonCode.far.ps1
}
