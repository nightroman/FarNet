
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"

use Framework\v4.0.30319 MSBuild

$Builds = @(
	'FarNet\FarNet.build.ps1'
	'PowerShellFar\PowerShellFar.build.ps1'
)

task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }

	Remove-Item -Force -Recurse -ErrorAction 0 `
	obj,
	FarNetAccord.sdf,
	$FarHome\FarNet\Modules\Explore\About-Explore.htm
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

task NuGet {
	# Test build of the sample modules, make sure they are alive
	Invoke-Build TestBuild Modules\Modules.build.ps1

	# Call
	foreach($_ in $Builds) { Invoke-Build NuGet, Clean $_ }

	# Move result archives
	Move-Item FarNet\FarNet.*.nupkg, PowerShellFar\FarNet.PowerShellFar.*.nupkg $Home -Force
}
