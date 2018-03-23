
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

task Build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec {MSBuild FarNet.Demo.csproj /p:FarHome=$FarHome /p:FarNetModules=$FarNetModules /p:Configuration=Release}
}

task Clean {
	remove bin, obj
}

task . Build, Clean
