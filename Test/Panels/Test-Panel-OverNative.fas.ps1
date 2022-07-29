<#
.Synopsis
	FarNet panel over TmpPanel

.Description
	It opens the Temp panel then opens the FarNet panel and closes it. As a
	result, we get the file system panel, that is the Temp panel is closed.
	The case used to fail on closing the FarNet panel.

	Test _110201_111328

	160405 http://forum.farmanager.com/viewtopic.php?f=8&t=10191
	Yet another crash, not related, dealing with Descriptions and x64.
#>

job {
	# make a file
	Set-Content C:\TEMP\temp.temp $env:FarNetCode\Test\Panels\Test-Panel-OverNative.fas.ps1

	# go to the .temp file
	$Far.Panel.GoToPath('C:\TEMP\temp.temp')
	Assert-Far -FileName 'temp.temp'
}

# enter the .temp file
keys Enter
job {
	Assert-Far -Plugin
	Assert-Far ($Far.Panel.GetFiles()[0].Name -eq "$env:FarNetCode\Test\Panels\Test-Panel-OverNative.fas.ps1")
}

job {
	# open the FarNet panel
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
}
job {
	Assert-Far -Plugin
	Assert-Far ($Far.Panel -is [FarNet.Panel])
}

# close the FarNet panel
macro 'Keys"Esc 1"'

# end
job {
	Remove-Item 'C:\TEMP\temp.temp'
}
