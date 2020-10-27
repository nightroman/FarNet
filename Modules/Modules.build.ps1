<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome)
)

$Builds = @(
	'Backslash\.build.ps1'
	'EditorKit\.build.ps1'
	'FarNet.Demo\.build.ps1'
	'TryPanelCSharp\.build.ps1'
)

task testBuild {
	# build
	$FarNetModules = 'C:\TEMP\z'
	foreach($_ in $Builds) { Invoke-Build build $_ }

	# test
	assert (Test-Path $FarNetModules\Backslash\Backslash.dll)
	assert (Test-Path $FarNetModules\EditorKit\EditorKit.dll)
	assert ((Get-Item $FarNetModules\FarNet.Demo\*).Count -eq 5)
	assert (Test-Path $FarNetModules\TryPanelCSharp\TryPanelCSharp.dll)

	# clean
	Remove-Item $FarNetModules -Recurse -Force
},
clean

task clean {
	foreach($_ in $Builds) { Invoke-Build clean $_ }
}
