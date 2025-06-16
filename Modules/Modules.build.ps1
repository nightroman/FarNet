<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
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
	equals (Get-Item $FarNetModules\FarNet.Demo\*).Count 6
	requires -Path @(
		"$FarNetModules\Backslash\Backslash.dll"
		"$FarNetModules\IronPythonFar\IronPythonFar.dll"
		"$FarNetModules\TryPanelCSharp\TryPanelCSharp.dll"
	)

	# clean
	remove $FarNetModules
},
clean

task clean {
	foreach($_ in $Builds) { Invoke-Build clean $_ }
	remove *\obj
}

task . testBuild
