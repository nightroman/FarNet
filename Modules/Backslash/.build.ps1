
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

$dir = "$FarNetModules\Backslash"
$src = 'Backslash.cs'
$dll = 'Backslash.dll'

use Framework\v3.5 csc

task Build -Inputs $src -Outputs $dll {
	exec { csc /target:library /optimize "/reference:$FarHome\FarNet\FarNet.dll" *.cs }
	$null = mkdir $dir -Force
	Copy-Item $dll $dir
}

task Clean {
	Remove-Item $dll -ErrorAction 0
}
