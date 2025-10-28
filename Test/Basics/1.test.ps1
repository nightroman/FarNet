Set-StrictMode -Version 3

### Auto vars

task auto_var_area {
	equals $__.GetType().FullName FarNet.Panel1

	$__ = 42
	equals $__ 42

	try { throw $Global:__ = 42 }
	catch { equals $_.FullyQualifiedErrorId VariableNotWritable }

	$Error.RemoveAt(0)
}

task auto_var_path {
	assert ($_path -like '?:\*')

	$_path = 42
	equals $_path 42

	try { throw $Global:_path = 42 }
	catch { equals $_.FullyQualifiedErrorId VariableNotWritable }

	$Error.RemoveAt(0)
}

###

# Cover crash on invalid non-modal dialog parameters and leaks after fixing the crash
# https://github.com/FarGroup/FarManager/issues/66
task 3_0_5261_crash {
	Assert-Far -Title Ensure -NoError
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
