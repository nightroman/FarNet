<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release),
	$TargetFramework = (property TargetFramework net6.0)
)
$FarHome = "C:\Bin\Far\$Platform"

task clean {
	remove bin, obj
}

task install {
	$dir = "$FarHome\FarNet"
	$null = mkdir $dir -Force
	Copy-Item -Destination $dir @(
		"bin\$Configuration\$TargetFramework\FarNet.dll"
		"bin\$Configuration\$TargetFramework\FarNet.xml"
	)
}

task uninstall {
	remove $FarHome\FarNet\FarNet.dll, $FarHome\FarNet\FarNet.xml
}
