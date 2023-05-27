<#
.Synopsis
	Fixed _090321_210416
#>

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
