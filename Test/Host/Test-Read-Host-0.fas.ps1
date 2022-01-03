<#
.Synopsis
	Read-Host empty prompt and [Esc]

.Description
	This scenario calls ReadLine(), not Prompt()
#>

### Test Esc

run {
	# show prompt
	$r = Read-Host
	Assert-Far $r -eq $null
}
job {
	Assert-Far -Dialog
}
keys Esc
job {
	Assert-Far -Panels
}

### Test Enter

run {
	# show prompt
	$r = Read-Host
	Assert-Far $r -eq 140302_142029
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq ''

	$Far.Dialog[1].Text = '140302_142029'
}
keys Enter
