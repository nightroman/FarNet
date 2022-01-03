<#
.Synopsis
	Read-Host 1 line prompt
#>

job { [PowerShellFar.Zoo]::StartCommandConsole() }

run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$Far.Dialog[0].Text = 'Read-Host -Prompt Prompt'
}
keys Enter
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Prompt'
	Assert-Far $Far.Dialog[1].Text -eq ': '
	$Far.Dialog[0].Text = '090328_192727'
}
keys Enter
job {
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq ': 090328_192727'
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '090328_192727'
}

job { [PowerShellFar.Zoo]::ExitCommandConsole() }
