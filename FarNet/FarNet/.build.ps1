
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"

task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
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
	Remove-Item $FarHome\FarNet\FarNet.dll, $FarHome\FarNet\FarNet.xml -ErrorAction 0
}
