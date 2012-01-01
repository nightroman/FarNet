
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$Configuration = (property Configuration Release),
	$Platform = 'Win32'
)

$CopyFile = 'FarNetMan.hlf', "$Configuration\$Platform\FarNetMan.dll"

task Clean {
	Remove-Item Debug, Release -Force -Recurse -ErrorAction 0
}

task Install {
	$dir = "$FarHome\Plugins\FarNet"
	$null = mkdir $dir -Force
	Copy-Item -LiteralPath $CopyFile $dir
}

task Uninstall {
	Remove-Item $FarHome\Plugins\FarNet -Force -Recurse -ErrorAction 0
}
