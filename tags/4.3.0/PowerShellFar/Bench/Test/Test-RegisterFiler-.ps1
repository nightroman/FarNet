
<#
.SYNOPSIS
	Test a filer handler.
	Author: Roman Kuzmin

.DESCRIPTION
	Steps:
	- invoke this script, it registers a *.test files handler;
	- [Enter] on a text file *.test to view its lines in a panel;
	- invoke this script again, the handler should be unregistered.
#>

if (!$TestFiler) {

	# install the handler
	$global:TestFiler = {&{
		# get and show all lines in a panel
		$p = New-Object PowerShellFar.UserPanel
		$p.Panel.Info.Title = $_.Name
		$p.Panel.Info.HostFile = $_.Name
		$p.Panel.Info.StartSortMode = 'Unsorted'
		$p.AddObjects([IO.File]::ReadAllLines($_.Name))
		$p.Show()
	}}

	# register the handler
	$Far.RegisterFiler($null, "PSF test filer", $TestFiler, "*.test", $false)
	Show-FarMsg "Test filer is registered. [Enter] *.test files now."
}
else {
	# unregister and uninstall the handler
	$Far.UnregisterFiler($TestFiler)
	Remove-Variable TestFiler -Scope Global
	Show-FarMsg "Test filer is unregistered"
}
