<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release),
	$FarNetModules = (property FarNetModules $FarHome\FarNet\Modules)
)

Set-StrictMode -Version 3
$ModuleName = 'IronPythonFar'
$ModuleHome = "$FarNetModules\$ModuleName"

task build {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome /p:FarNetModules=$FarNetModules }
}

task publish {
	exec { dotnet publish "$ModuleName.csproj" -c $Configuration -o $ModuleHome --no-build }

	Set-Location $ModuleHome
	Remove-Item "$ModuleName.deps.json"

	Set-Location runtimes
	remove android*, ios*, linux*, mac*, osx*, tvos*, unix
}

task clean {
	remove z, bin, obj
}

task . build, clean
