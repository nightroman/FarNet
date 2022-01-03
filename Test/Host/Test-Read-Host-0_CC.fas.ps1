<#
.Synopsis
	Read-Host empty prompt and [Esc]

.Description
	This scenario calls ReadLine(), not Prompt()
#>

job { [PowerShellFar.Zoo]::StartCommandConsole() }

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
	Assert-Far $Far.Dialog[1].Text -eq ''
	$Far.Dialog[0].Text = '140302_142029'
}
keys Enter

job { [PowerShellFar.Zoo]::ExitCommandConsole() }
