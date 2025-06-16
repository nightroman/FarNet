<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

$fa_outdir = "$FarHome\FarNet"

task clean {
	remove bin, obj
}

task install

task uninstall {
	remove "$fa_outdir\FarNet.dll", "$fa_outdir\FarNet.xml"
}
