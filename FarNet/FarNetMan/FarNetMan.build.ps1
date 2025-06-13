<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

$To_Plugins_FarNet = "$FarHome\Plugins\FarNet"

task clean {
	remove Debug, Release, FarNetMan.vcxproj.user
}

task install {
	Set-Location .\$Configuration\$Platform
	$null = mkdir $To_Plugins_FarNet -Force
	Copy-Item -Destination $To_Plugins_FarNet @(
		"FarNetMan.dll"
		"FarNetMan.pdb"
		"FarNetMan.runtimeconfig.json"
		"Ijwhost.dll"
	)
}

task uninstall {
	remove $To_Plugins_FarNet
}
