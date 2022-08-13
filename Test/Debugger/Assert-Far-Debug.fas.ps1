<#
.Synopsis
	Tests [Debug] with Add-Debugger.ps1, _220809_2057
#>

run {
	Add-Debugger.ps1 -ReadHost
	Assert-Far $false
	throw 'unexpected'
}

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq Assert-Far
}

keys d

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Step (h or ? for help)'
	$Far.Dialog[2].Text = 'c'
	$Far.Dialog.Close()
}

job {
	Restore-Debugger

	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq Assert-Far
	$Far.Dialog.Close()
}
