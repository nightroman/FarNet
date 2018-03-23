
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"

task Clean {
	remove bin, obj
}

task Install {
	$dir = "$FarHome\FarNet"
	$null = mkdir $dir -Force
	Copy-Item Bin\$Configuration\FarNet.dll $dir
	if (Test-Path Bin\$Configuration\FarNet.xml) {
		Copy-Item Bin\$Configuration\FarNet.xml $dir
	}
}

task Uninstall {
	remove $FarHome\FarNet\FarNet.dll, $FarHome\FarNet\FarNet.xml
}
