
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

$dir = "$FarNetModules\EditorKit"
$src = 'EditorKit.cs'
$dll = 'EditorKit.dll'

use $null csc

task Build -Incremental @{$src = $dll} {
	exec { csc /target:library /optimize "/reference:$FarHome\FarNet\FarNet.dll" *.cs }
}

task Clean {
	Remove-Item $dll -ErrorAction 0
}

task Install {
	$null = mkdir $dir -Force
	Copy-Item $dll $dir
}
