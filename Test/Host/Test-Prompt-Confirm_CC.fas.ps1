<#
.Synopsis
	PromptForChoice()
#>

job { $Psf.RunCommandConsole() }

### Test Esc

run {
	# trigger confirm dialog
	$global:090328194636 = 42
	Remove-Item Variable:\090328194636 -Confirm
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
}
keys Esc
job {
	# variable exists
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	Assert-Far $global:090328194636 -eq 42
}

### Test `?` and `y`

run {
	# trigger confirm dialog
	Remove-Item Variable:\090328194636 -Confirm
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
}
keys ? Enter
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.UI.GetBufferLineText(-4) -eq 'S - Pause the current pipeline and return to the command prompt. Type "exit" to resume the pipeline.'
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq 'Escape - stop the pipeline.'
}
keys Enter
job {
	# variable removed
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	Assert-Far (!(Test-Path Variable:\090328194636))
}

job {
	$Psf.StopCommandConsole()
	[FarNet.Tasks]::WaitForPanels(9)
}

