<#
.Synopsis
	Open a panel when two panels are opened, get its data.
#>

# open 2 test panels
job {
	& "$env:PSF\Samples\Tests\Test-Panel.far.ps1"
}
keys Tab
job {
	& "$env:PSF\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far @(
		$Far.Panel.Title -eq 'Test Panel'
		$Far.Panel2.Title -eq 'Test Panel'
	)
}

# open yet another panel; NOTE: the current panel has gone
job {
	([PowerShellFar.PowerExplorer][guid]'f8e30845-abb4-4a51-9c08-e07c602f3610').OpenPanel()
}
job {
	Assert-Far -ExplorerTypeId f8e30845-abb4-4a51-9c08-e07c602f3610
}

# exit another panel; NOTE: Far panel
keys Esc
job {
	Assert-Far -Native
}

# go to panel 2
keys Tab
# close the panel, mind a dialog
keys Esc
job {
	Assert-Far -Dialog
}
keys 1
