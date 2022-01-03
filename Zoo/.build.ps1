
param(
	[ValidateSet('Win32', 'x64')]
	$Platform = (property Platform x64),
	[ValidateSet('Release', 'Debug')]
	$Configuration = (property Configuration Release),
	$FarFarm = "C:\-\GIT\FarManager"
)
$FarHome = "C:\Bin\Far\$Platform"
$Bitness = if ($Platform -eq 'Win32') {32} else {64}

Set-Alias MSBuild (Resolve-MSBuild)

# Zip FarDev sources on release.
task zipFarDev {
	#fix hardcoded path
	. $env:FarNetCode\Get-Version.ps1
	#! use a file not in FarDev
	$zip = "C:\ROM\z\__bak\FarDev.$FarNetVersion-$PowerShellFarVersion.7z"
	Set-Location ..
	if (Test-Path $zip) { Remove-Item $zip -Confirm }
	exec { & 7z.exe a $zip FarDev '-xr!.vs' '-xr!bin' '-xr!obj' '-xr!packages' }
}

# Interactive release steps
task release {
	#! save to external file or it goes to zip
	$save = "$HOME\ReleaseFarNet.clixml"
	Build-Checkpoint $save @{Task = '*'; File = 'ReleaseFarNet.build.ps1'} -Resume:(Test-Path $save)
}

# Pack main FarNet assets
task nuget {
	Invoke-Build NuGet $env:FarNetCode\.build.ps1
}

# Test main FarNet assets
task testNuGet {
	.\Test-Update-FarNet.ps1
}
