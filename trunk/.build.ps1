
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$Configuration = (property Configuration Release),
	$Platform = 'Win32'
)

use Framework\v4.0.30319 MSBuild

$Builds = @(
	'FarNet\FarNet.build.ps1'
	'PowerShellFar\PowerShellFar.build.ps1'
)

task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
	Remove-Item FarNetAccord.sdf -ErrorAction 0
}

task Build {
	exec { MSBuild FarNetAccord.sln /t:Build /p:Configuration=$Configuration /p:Platform=$Platform }
}

task Install {
	foreach($_ in $Builds) { Invoke-Build Install $_ }
}

task Uninstall {
	foreach($_ in $Builds) { Invoke-Build Uninstall $_ }
}

task Zip {
	. .\Get-Version.ps1

	# Test build of the sample modules, make sure they are alive
	Invoke-Build TestBuild Modules\Modules.build.ps1

	# Call
	foreach($_ in $Builds) { Invoke-Build Zip $_ }

	# Zip FarNetAccord
	Remove-Item [z] -Recurse -Force
	$null = mkdir z\FarNet
	Copy-Item Install.txt z
	Copy-Item $FarHome\FarNet\FarNetAccord.chm z\FarNet
	Push-Location z
	exec { & 7z a ..\FarNetAccord.$FarNetAccordVersion.7z * }
	Pop-Location
	Remove-Item z -Recurse -Force

	# Move result archives
	Move-Item FarNetAccord.*.7z, FarNet\FarNet.*.7z, PowerShellFar\PowerShellFar.*.7z $Home -Force
}
