<#
.Synopsis
	SecureString input
.Description
	We do not test [Esc] because used Prompt() is tested by other tests.
#>

### Empty prompt, calls ReadLineAsSecureString()

run {
	$ss = Read-Host -AsSecureString
	Assert-Far -Panels
	Assert-Far ($ss -is [System.Security.SecureString])
}
job {
	$dialog = $Far.Dialog
	Assert-Far @(
		$dialog
		$dialog[1].Text -eq ' '
		$dialog[2].GetType().Name -eq 'FarEdit'
		$dialog[2].IsPassword
	)
}
keys p a s s Enter

### Not empty prompt, calls Prompt()
run {
	$ss = Read-Host -AsSecureString 'Password'
	Assert-Far -Panels
	Assert-Far ($ss -is [System.Security.SecureString])
}
job {
	$dialog = $Far.Dialog
	Assert-Far @(
		$dialog
		$dialog[1].Text -eq 'Password'
		$dialog[2].GetType().Name -eq 'FarEdit'
		$dialog[2].IsPassword
	)
}
keys p a s s Enter
