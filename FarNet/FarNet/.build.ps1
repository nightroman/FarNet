<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release),
	$TargetFramework = (property TargetFramework net8.0)
)

task clean {
	remove bin, obj
}

task install

task uninstall {
	remove $FarHome\FarNet\FarNet.dll, $FarHome\FarNet\FarNet.xml
}
