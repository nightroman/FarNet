<#
.Synopsis
	2009-03-21-1656
#>

job {
	# open module panel
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}

job {
	# add panel Closing
	Assert-Far -Plugin
	$__.add_Closing({
		$DebugPanelClosing = 42
	})

	# set a breakpoint to watch Closing
	(Get-PSBreakpoint).Where{$_.Variable -eq 'DebugPanelClosing'}.ForEach{Remove-PSBreakpoint $_}
	$Data.bp = Set-PSBreakpoint -Variable 'DebugPanelClosing' -Action {}
}

# invoke any plugin commands from cmdline
keys p s : Enter
keys p s : Enter

job {
	# fixed Far 3.0.6227: check hit count: expected 0, actual 2
	Assert-Far $Data.bp.HitCount -eq 0
}

# exit the panel, mind the dialog
keys Esc 2

job {
	Assert-Far -Native
	Assert-Far $Data.bp.HitCount -eq 1
	Remove-PSBreakpoint -Breakpoint $Data.bp
}
