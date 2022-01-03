<#
.Synopsis
	Cover/check panel issues.
#>

### Check: [_090321_210416]

job {
	# go to the folder with some files
	$Far.Panel.CurrentDirectory = 'C:\ROM\APS'
}
job {
	Assert-Far $Far.Panel.CurrentIndex -eq 0
}
job {
	# open the panel
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
}
# add 2 items, go to end
macro 'Keys"F7 F7 End"'
job {
	Assert-Far $Far.Panel.CurrentIndex -eq 2
}
# close the panel, choose '1' in the dialog
macro 'Keys"Esc 1"'
job {
	# exited?
	Assert-Far -Native
	# _090321_210416 fixed: index used to be still 2; it is 0 now!
	Assert-Far $Far.Panel.CurrentIndex -eq 0
}

### Check: [_090321_165608]

job {
	# set a breakpoint to watch Closing and open the panel
	Get-PSBreakpoint | .{process{ if ($_.Variable -eq 'DebugPanelClosing') { Remove-PSBreakpoint $_ } }}
	$null = Set-PSBreakpoint -Variable 'DebugPanelClosing' -Action {}
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
}

# invoke innocent plugin commands from cmdline
macro 'Keys"p s : Enter"'
macro 'Keys"p s : Enter"'

job {
	# check breakpoint hit count: I want it to be 0, but it is 2
	$bp = Get-PSBreakpoint | .{process{ if ($_.Variable -eq 'DebugPanelClosing') { $_ } }}
	Assert-Far $bp.HitCount -eq 2
	Remove-PSBreakpoint $bp
}

# exit the panel, mind the dialog
macro 'Keys"Esc 2"'
job {
	Assert-Far -Native
}

### Cover: open a panel when two panels are opened, get its data

# open 2 test panels
job {
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
}
keys Tab
job {
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
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
