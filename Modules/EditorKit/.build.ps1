
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

[CmdletBinding()]
param(
	$FarHome = (property FarHome C:\Bin\Far\Win32),
	$Configuration = (property Configuration Release),
	$FarNetModules = (property FarNetModules $FarHome\FarNet\Modules)
)

$ModuleName = 'EditorKit'
$ProjectRoot = '.'
$ProjectName = "$ModuleName.csproj"

task Build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec {MSBuild $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration /p:FarNetModules=$FarNetModules}
}

task Clean {
	Get-Item $ProjectRoot\bin, $ProjectRoot\obj -ErrorAction 0 | Remove-Item -Force -Recurse
}

task . Build, Clean
