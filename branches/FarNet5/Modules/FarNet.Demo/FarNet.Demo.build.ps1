
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

use Framework\v4.0.30319 MSBuild

task Build {
	exec { MSBuild /t:Build "/p:Configuration=Release;Install=$FarNetModules" FarNet.Demo.csproj }
}

task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task Install
