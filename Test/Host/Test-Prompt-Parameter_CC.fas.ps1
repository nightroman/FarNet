<#
.Synopsis
	Prompt() with one field
#>

job { $Psf.RunCommandConsole() }

run {
	# remove the variable
	if (Test-Path Variable:\090328194636) { Remove-Item Variable:\090328194636 }

	# trigger prompt dialog
	$null = New-Variable -Scope global
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq 'cmdlet New-Variable at command pipeline position 1'
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Supply values for the following parameters:'
	Assert-Far $__[0].Text -eq 'Name: '
}
keys 0 9 0 3 2 8 1 9 4 6 3 6 Enter
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Name: 090328194636'

	# variable exist
	Assert-Far (Test-Path Variable:\090328194636)
	Remove-Variable -Scope Global 090328194636
}

job {
	$Psf.StopCommandConsole()
	[FarNet.Tasks]::WaitForPanels(9)
}
