<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome)
)

$Builds = Get-Item *\*.build.ps1

task testBuild {
	# build
	$FarNetModules = 'C:\TEMP\z'
	foreach($_ in $Builds) { Invoke-Build build $_ }

	# test
	assert (Test-Path $FarNetModules\Backslash\Backslash.dll)
	assert ((Get-Item $FarNetModules\FarNet.Demo\*).Count -eq 5)
	assert (Test-Path $FarNetModules\IronPythonFar\IronPythonFar.dll)
	assert (Test-Path $FarNetModules\JavaScriptFar\JavaScriptFar.dll)
	assert (Test-Path $FarNetModules\TryPanelCSharp\TryPanelCSharp.dll)

	# clean
	remove $FarNetModules
},
clean

task clean {
	foreach($_ in $Builds) { Invoke-Build clean $_ }
}
