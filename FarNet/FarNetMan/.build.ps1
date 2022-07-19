<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Configuration = (property Configuration Release),
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform")
)

$ModuleHome = "$FarHome\Plugins\FarNet"

task clean {
	remove Debug, Release, FarNetMan.vcxproj.user
}

task install {
	$null = mkdir $ModuleHome -Force
	Copy-Item -Destination $ModuleHome @(
		"$Configuration\$Platform\FarNetMan.dll"
		"$Configuration\$Platform\FarNetMan.pdb"
		"$Configuration\$Platform\FarNetMan.runtimeconfig.json"
		"$Configuration\$Platform\Ijwhost.dll"
	)
}

task uninstall {
	remove $FarHome\Plugins\FarNet
}
