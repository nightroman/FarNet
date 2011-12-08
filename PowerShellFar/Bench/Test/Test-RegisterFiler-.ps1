
<#
.Synopsis
	Test a filer handler.
	Author: Roman Kuzmin

.Description
	Steps:
	- invoke this script, it registers a *.test files handler;
	- [Enter] on a text file *.test to view its lines in a panel;
	- invoke this script again, the handler should be unregistered.
#>

$filer = $Far.GetModuleFiler("d7fb89f3-b24b-40f1-b94b-83031d87bf52")
if ($filer) {
	# unregister
	$filer.Unregister()
	Show-FarMessage "Test filer is unregistered"
}
else {
	# register
	$null = $Psf.Manager.RegisterModuleFiler(
		"d7fb89f3-b24b-40f1-b94b-83031d87bf52",
		(New-Object FarNet.ModuleFilerAttribute -Property @{ Name = "PSF test filer"; Mask = "*.test" }),
		{&{
			# get and show all lines in a panel
			$Panel = New-Object PowerShellFar.ObjectPanel
			$Panel.Title = $_.Name
			$Panel.HostFile = $_.Name
			$Panel.SortMode = 'Unsorted'
			$Panel.AddObjects([IO.File]::ReadAllLines($_.Name))
			$Panel.Open()
		}}
	)
	Show-FarMessage "Test filer is registered. [Enter] *.test files now."
}
