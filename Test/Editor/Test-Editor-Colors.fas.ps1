<#
.Synopsis
	Test editor colors
#>

$Data.MacroOutput = [FarNet.Works.Kit]::MacroOutput
[FarNet.Works.Kit]::MacroOutput = $true

job {
	& "$env:PSF\Samples\Tests\Test-RegisterDrawer-.ps1"
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.IsLocked
	$data = Show-EditorColor-.ps1

	#! dump size; NB: it is larger is call manually (why?)
	#! the number maybe different depending on planets
	Assert-Far 290 -eq $data.Count

	# from Colorer
	if (882 -eq $data.Count) {
		Assert-Far $data -eq '0 d2f36b62-a470-418d-83a3-ed7a3710e5b5 0 (1, 2) Black/White                      : 0'
	}

	# from test
	Assert-Far ($data -eq '1 4ddb64b8-7954-41f0-a93f-d5f6a09cc752 0 (0, 3) Black/Black                      :  0 ')
	Assert-Far ($data -eq '1 4ddb64b8-7954-41f0-a93f-d5f6a09cc752 11 (30, 33) Green/Cyan                    :  A ')
}
job {
	$Far.Editor.Close()
}
job {
	Assert-Far -Panels
	Assert-Far (!(Test-Path $env:TEMP\Colors))
	$drawer = $Far.GetModuleAction('4ddb64b8-7954-41f0-a93f-d5f6a09cc752')
	$drawer.Unregister()

	[FarNet.Works.Kit]::MacroOutput = $Data.MacroOutput
}
