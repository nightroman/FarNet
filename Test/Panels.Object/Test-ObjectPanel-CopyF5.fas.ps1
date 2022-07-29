<#
.Synopsis
	_100227_073909 Copy from an object panel
#>

job {
	# open an object source panel
	42 | Out-FarPanel
}
job {
	Assert-Far -ExplorerTypeId ([PowerShellFar.Guids]::ObjectExplorer)
	Find-FarFile 42
}

# open another object target panel
keys Tab
job {
	Out-FarPanel
}
job {
	Assert-Far -ExplorerTypeId ([PowerShellFar.Guids]::ObjectExplorer)
}

# go back to the source
keys Tab
job {
	Assert-Far @(
		$Far.Panel.GetFiles().Count -eq 1
		$Far.Panel2.GetFiles().Count -eq 0
	)
}

# copy
keys F5
job {
	Assert-Far @(
		$Far.Panel.GetFiles().Count -eq 1
		$Far.Panel2.GetFiles().Count -eq 1
	)
}

# exit both panels
keys Tab Esc Tab Esc
