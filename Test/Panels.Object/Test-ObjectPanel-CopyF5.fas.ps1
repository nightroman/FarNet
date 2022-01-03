<#
.Synopsis
	_100227_073909 Copy from an object panel
#>

job {
	# open an object source panel
	Get-Process Far | Out-FarPanel
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	Find-FarFile Far
}

# open another object target panel
keys Tab
job {
	Out-FarPanel
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
}

# go back to the source
keys Tab
job {
	Assert-Far @(
		$Far.Panel.ShownFiles.Count -eq 1
		$Far.Panel2.ShownFiles.Count -eq 0
	)
}

# copy
keys F5
job {
	Assert-Far @(
		$Far.Panel.ShownFiles.Count -eq 1
		$Far.Panel2.ShownFiles.Count -eq 1
	)
}

# exit the target and source panels
macro 'Keys"Tab Esc Tab Esc"'
