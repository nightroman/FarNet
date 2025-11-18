<#
.Synopsis
	Read-Host 1 line prompt
#>

job { $Psf.RunCommandConsole() }

run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$Far.Dialog[1].Text = 'Read-Host -Prompt Prompt'
}
keys Enter
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.Dialog[0].Text -eq 'Prompt: '
	$Far.Dialog[1].Text = '090328_192727'
}
keys Enter
job {
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq 'Prompt: 090328_192727'
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '090328_192727'
}

job {
	$Psf.StopCommandConsole()
	[FarNet.Tasks]::WaitForPanels(9)
}
