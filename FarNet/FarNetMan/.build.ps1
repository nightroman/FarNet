
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"

$CopyFile = "$Configuration\$Platform\FarNetMan.dll"

task Clean {
	remove Debug, Release, FarNetMan.vcxproj.user
}

task Install {
	$dir = "$FarHome\Plugins\FarNet"
	$null = mkdir $dir -Force
	Copy-Item -LiteralPath $CopyFile $dir
}

task Uninstall {
	remove $FarHome\Plugins\FarNet
}
