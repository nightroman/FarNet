
# Synopsis: Interactive release steps.
task release {
	#! save outside or it gets to zip
	Build-Checkpoint -Auto "$HOME\z.ReleaseFarNet.clixml" @{Task = '*'; File = 'ReleaseFarNet.build.ps1'}
}

# Synopsis: Pack FarNet assets.
task nuget {
	Invoke-Build nuget ..
}

# Synopsis: Test FarNet assets.
task testNuGet {
	.\Test-NuGet.ps1
}

# Synopsis: Zip FarDev sources on release.
task zipFarDev {
	. ..\Get-Version.ps1
	$zip = "FarDev.$FarNetVersion-$PowerShellFarVersion.7z"

	Set-Location ..\..\..
	assert (Test-Path FarDev)

	if (Test-Path $zip) { Remove-Item $zip -Confirm }
	exec { & 7z.exe a $zip FarDev '-xr!.vs' '-xr!bin' '-xr!obj' '-xr!packages' }
}
