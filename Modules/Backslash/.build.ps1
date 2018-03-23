
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release),
	$FarNetModules = (property FarNetModules $FarHome\FarNet\Modules)
)

$ModuleName = 'Backslash'
$ProjectRoot = '.'
$ProjectName = "$ModuleName.csproj"

task Build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec {MSBuild $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration /p:FarNetModules=$FarNetModules}
}

task Clean {
	remove $ProjectRoot\bin, $ProjectRoot\obj
}

task . Build, Clean
