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
$ModuleRoot = "$FarNetModules\$ModuleName"

task build {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome -p:FarNetModules=$FarNetModules }
}

task clean {
	remove z, bin, obj
}

task . build, clean
