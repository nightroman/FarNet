<#
.Synopsis
	Read-Host empty prompt and [Esc]

.Description
	This scenario calls ReadLine(), not Prompt()
#>

fun { $Psf.RunCommandConsole() }

### Test Esc

run {
	# show prompt
	$r = Read-Host
	Assert-Far $r -eq $null
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
}
keys Esc
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
}

### Test Enter

run {
	# show prompt
	$r = Read-Host
	Assert-Far $r -eq 140302_142029
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	$__[0].Text = '140302_142029'
}
keys Enter

keys Esc # exit CC
