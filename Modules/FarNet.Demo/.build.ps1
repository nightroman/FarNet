<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

task build {
	exec {&(Resolve-MSBuild) FarNet.Demo.csproj /p:FarHome=$FarHome /p:FarNetModules=$FarNetModules /p:Configuration=Release}
}

task clean {
	remove bin, obj
}

task . build, clean
