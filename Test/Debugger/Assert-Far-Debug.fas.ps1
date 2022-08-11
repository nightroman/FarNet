<#
.Synopsis
	Tests _psf_debug with Add-Debugger.ps1, _220809_2057
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
Start-Sleep 1
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Command (h or ? for help)'
	$Far.Dialog[1].Text = 'q'
	$Far.Dialog.Close()
}
job {
	Restore-Debugger
	Assert-Far (!(Test-Path Variable:\_psf_debug))
	Assert-Far (!(Get-PSBreakpoint -Variable _psf_debug))

	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq Assert-Far
	$Far.Dialog.Close()
}
