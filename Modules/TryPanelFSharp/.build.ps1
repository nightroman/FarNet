
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

$ModuleName = 'TryPanelFSharp'
$ProjectRoot = '.'
$ProjectName = "$ModuleName.fsproj"
$BinFiles = "$ModuleName.dll"

task . Build, Clean

task Build {
	use * MSBuild.exe
	exec {MSBuild.exe $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration /p:FarNetModules=$FarNetModules}
}

task PostBuild {
	exec {robocopy.exe $ProjectRoot\bin\$Configuration $FarNetModules\$ModuleName $BinFiles /NP /NJS /R:0} (0..3)
}

task Clean {
	Remove-Item $ProjectRoot\bin, $ProjectRoot\obj -Force -Recurse -ErrorAction 0
}
