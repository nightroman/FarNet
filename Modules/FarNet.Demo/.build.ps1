<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

Set-StrictMode -Version 2
$ModuleHome = "$FarNetModules\FarNet.Demo"

task build {
	exec { dotnet restore }
	exec { dotnet msbuild FarNet.Demo.csproj /p:FarHome=$FarHome /p:FarNetModules=$FarNetModules /p:Configuration=Release }
}

# https://github.com/nightroman/PowerShelf/blob/master/Invoke-Environment.ps1
task resgen @{
	Inputs = 'FarNet.Demo.restext', 'FarNet.Demo.ru.restext'
	Outputs = "$ModuleHome\FarNet.Demo.resources", "$ModuleHome\FarNet.Demo.ru.resources"
	Partial = $true
	Jobs = {
		begin {
			$VsDevCmd = @(Get-Item 'C:\Program Files (x86)\Microsoft Visual Studio\2019\*\Common7\Tools\VsDevCmd.bat')
			Invoke-Environment.ps1 -File ($VsDevCmd[0])
		}
		process {
			exec {resgen.exe $_ $2}
		}
	}
}

task publish resgen

task clean {
	remove bin, obj
}

task . build, clean
