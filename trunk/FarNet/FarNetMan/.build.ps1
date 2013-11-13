
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

$CopyFile = "$Configuration\$Platform\FarNetMan.dll"

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
