<#
.Synopsis
	Case _090321_165608
#>

job {
	# set a breakpoint to watch Closing and open the panel
	Get-PSBreakpoint | .{process{ if ($_.Variable -eq 'DebugPanelClosing') { Remove-PSBreakpoint $_ } }}
	$null = Set-PSBreakpoint -Variable 'DebugPanelClosing' -Action {}
	& "$env:PSF\Samples\Tests\Test-Panel.far.ps1"
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
