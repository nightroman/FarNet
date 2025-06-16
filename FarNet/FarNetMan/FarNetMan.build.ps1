<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

$fm_outdir = "$FarHome\Plugins\FarNet"

task clean {
	remove Debug, Release, FarNetMan.vcxproj.user
}

task install {
	$null = mkdir $fm_outdir -Force

	Set-Location .\$Configuration\$Platform
	Copy-Item -Destination $fm_outdir @(
		"FarNetMan.dll"
		"FarNetMan.pdb"
		"FarNetMan.runtimeconfig.json"
		"Ijwhost.dll"
	)
}

task uninstall {
	remove $fm_outdir
}
