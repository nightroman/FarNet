<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome),
	$FarNetModules = (property FarNetModules "$FarHome\FarNet\Modules")
)

Set-StrictMode -Version 3
$ModuleRoot = "$FarNetModules\FarNet.Demo"

task build {
	exec { dotnet build -c Release -p:FarHome=$FarHome -p:FarNetModules=$FarNetModules }
}

task publish resgen

task clean {
	remove obj
}

# https://github.com/nightroman/PowerShelf/blob/main/Invoke-Environment.ps1
task resgen @{
	Inputs = 'FarNet.Demo.restext', 'FarNet.Demo.ru.restext'
	Outputs = "$ModuleRoot\FarNet.Demo.resources", "$ModuleRoot\FarNet.Demo.ru.resources"
	Partial = $true
	Jobs = {
		begin {
			$VsDevCmd = @(Get-Item "$env:ProgramFiles\Microsoft Visual Studio\*\*\Common7\Tools\VsDevCmd.bat")
			Invoke-Environment.ps1 -File ($VsDevCmd[0])
		}
		process {
			exec {resgen.exe $_ $2}
		}
	}
}

task . build, clean
