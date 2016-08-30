
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

task Build {
	use * MSBuild.exe
	exec { MSBuild.exe FarNet.Demo.csproj /p:FarHome=$FarHome /p:FarNetModules=$FarNetModules /p:Configuration=Release }
}

task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task . Build, Clean
