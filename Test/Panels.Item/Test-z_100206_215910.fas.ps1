<#
.Synopsis
	Registry panel: PS doesn't keep the location on errors if $ErrorActionPreference = 'Stop'. _100206_215910

.Description
	Example:
	sl HKCU:\aa # gl gets HKCU:\aa
	sl 'a:|a' # peculiar but possible registry key name -- error
	gl # gets original FileSystem location, not 'HKCU:\aa'
	This and other issues were fixed by using full paths.
#>

job {
	# setup
	if (Test-Path 'hkcu:\a1\a:|a') { Remove-Item 'hkcu:\a1\a:|a' }
	elseif (!(Test-Path 'hkcu:\a1')) { $null = New-Item 'hkcu:\a1' }
	Go-To hkcu:\a1
}

# new item with the funny name
macro 'Keys"F7 a : | a Enter"'
job {
	Assert-Far -FileName 'a:|a'
}

# enter the funny key
keys Enter
job {
	Assert-Far $Far.Panel.CurrentFile -eq $null
}

# exit the funny key on dots
keys Enter
job {
	Assert-Far -FileName 'a:|a'
}

# step out
keys CtrlPgUp
job {
	Assert-Far @(
		Test-Path 'hkcu:\a1'
		(Get-FarItem).Name -eq 'HKEY_CURRENT_USER\a1'
	)
}

# delete the key a1 (with children)
keys Del y Enter y Enter
job {
	Assert-Far (!(Test-Path 'hkcu:\a1'))
}

# end
keys Esc
