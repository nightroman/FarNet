
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome)
)

$IsFSharp = (
	(Test-Path "${env:ProgramFiles(x86)}\Microsoft SDKs\F#\4.0\Framework\v4.0\Fsc.exe") -or
	(Test-Path "${env:ProgramFiles}\Microsoft SDKs\F#\4.0\Framework\v4.0\Fsc.exe")
)

$Builds = @(
	'Backslash\.build.ps1'
	'EditorKit\.build.ps1'
	'FarNet.Demo\.build.ps1'
	'TryPanelCSharp\.build.ps1'
	if ($IsFSharp) {
		'TryPanelFSharp\.build.ps1'
	}
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
	if ($IsFSharp) {
		assert (Test-Path $FarNetModules\TryPanelFSharp\TryPanelFSharp.dll)
	}

	# clean
	Remove-Item $FarNetModules -Recurse -Force
},
Clean

task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
}
