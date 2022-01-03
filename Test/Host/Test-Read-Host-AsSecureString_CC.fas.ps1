<#
.Synopsis
	SecureString input
#>

job { [PowerShellFar.Zoo]::StartCommandConsole() }

### Empty prompt, calls ReadLineAsSecureString()

run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$ss1 = Read-Host -AsSecureString
	Assert-Far ($ss1 -is [System.Security.SecureString])
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.Dialog[0].IsPassword
	Assert-Far $Far.Dialog[1].Text -eq ': '
}
keys p a s s Enter

### Not empty prompt, calls Prompt()
run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$ss2 = Read-Host -AsSecureString 'Password'
	Assert-Far ($ss2 -is [System.Security.SecureString])
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.Dialog[0].IsPassword
	Assert-Far $Far.Dialog[1].Text -eq ': '
}
keys p a s s Enter

job { [PowerShellFar.Zoo]::ExitCommandConsole() }
