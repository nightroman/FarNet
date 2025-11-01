<#
.Synopsis
	PromptForChoice()
#>

### Test Esc

run {
	# trigger confirm dialog
	$global:090328194636 = 42
	Remove-Item Variable:\090328194636 -Confirm
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Confirm'
}
keys Esc
job {
	# variable exists
	Assert-Far -Panels
	Assert-Far $global:090328194636 -eq 42
}

### Test `?` and `y`

run {
	# trigger confirm dialog
	Remove-Item Variable:\090328194636 -Confirm
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Confirm'
}
keys ? Enter
job {
	Assert-Far -Viewer
}
keys Esc
job {
	Assert-Far -Dialog
}
keys a Enter
job {
	# variable removed
	Assert-Far -Panels
	Assert-Far (!(Test-Path Variable:\090328194636))
}
