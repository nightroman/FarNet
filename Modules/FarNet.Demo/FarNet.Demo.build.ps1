
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

use 4.0 MSBuild

task Build {
	exec { MSBuild /t:Build "/p:Configuration=Release;Install=$FarNetModules;FarHome=$FarHome" FarNet.Demo.csproj }
}

task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task Install
