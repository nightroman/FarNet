<#
.Synopsis
	Prompt() with one field
#>

run {
	# remove the variable
	if (Test-Path Variable:\090328194636) { Remove-Item Variable:\090328194636 }

	# trigger prompt dialog
	$null = New-Variable -Scope global
}
job {
	Assert-Far ($Far.Dialog[0].Text -match '^cmdlet New-Variable')
}
keys 0 9 0 3 2 8 1 9 4 6 3 6 Enter
job {
	# variable exist
	Assert-Far -Panels
	Assert-Far (Test-Path Variable:\090328194636)
	Remove-Variable -Scope Global 090328194636
}
