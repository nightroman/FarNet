
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

task TestBuild {
	# build
	$FarNetModules = 'C:\TEMP\z'
	foreach($_ in $Builds) { Invoke-Build Build $_ }

	# test
	assert (Test-Path $FarNetModules\Backslash\Backslash.dll)
	assert (Test-Path $FarNetModules\EditorKit\EditorKit.dll)
	assert ((Get-Item $FarNetModules\FarNet.Demo\*).Count -eq 5)
	assert (Test-Path $FarNetModules\TryPanelCSharp\TryPanelCSharp.dll)

	# clean
	Remove-Item $FarNetModules -Recurse -Force
},
Clean

task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
}
