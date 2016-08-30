
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
	use * MSBuild.exe
	exec {MSBuild.exe $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration /p:FarNetModules=$FarNetModules}
}

task Clean {
	Remove-Item $ProjectRoot\bin, $ProjectRoot\obj -Force -Recurse -ErrorAction 0
}

task . Build, Clean
