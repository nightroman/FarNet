
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

$Builds = @(
	'Backslash\Backslash.build.ps1'
	'EditorKit\EditorKit.build.ps1'
	'FarNet.Demo\FarNet.Demo.build.ps1'
)

task TestBuild {
	# property
	$FarNetModules = 'C:\TEMP\z'

	# build, install
	foreach($_ in $Builds) { Invoke-Build Build, Install $_ }

	# test
	assert (Test-Path $FarNetModules\Backslash\Backslash.dll)
	assert (Test-Path $FarNetModules\EditorKit\EditorKit.dll)
	assert ((Get-Item $FarNetModules\FarNet.Demo\*).Count -eq 5)

	# clean
	Remove-Item $FarNetModules -Recurse -Force
},
Clean

task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
}
