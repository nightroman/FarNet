<#
.Synopsis
	SecureString input
#>

job { $Psf.RunCommandConsole() }

### Empty prompt, calls ReadLineAsSecureString()

run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$ss1 = Read-Host -AsSecureString
	Assert-Far ($ss1 -is [System.Security.SecureString])
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $__[0].IsPassword
}
keys p a s s Enter
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq '*'
}

### Not empty prompt, calls Prompt()
run {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	$ss2 = Read-Host -AsSecureString 'Password'
	Assert-Far ($ss2 -is [System.Security.SecureString])
}
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $__[0].Text -eq 'Password: '
	Assert-Far $__[1].IsPassword
}
keys p a s s Enter
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Password: *'
}

job { $Psf.StopCommandConsole() }
Start-Sleep 1 #TODO
