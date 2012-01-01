
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$Configuration = (property Configuration Release)
)

use Framework\v4.0.30319 MSBuild

$script:Builds = @(
	'FarNet\.build.ps1'
	'FarNet.Settings\.build.ps1'
	'FarNet.Tools\.build.ps1'
	'FarNet.Works.Config\.build.ps1'
	'FarNet.Works.Dialog\.build.ps1'
	'FarNet.Works.Editor\.build.ps1'
	'FarNet.Works.Macros\.build.ps1'
	'FarNet.Works.Manager\.build.ps1'
	'FarNet.Works.Panels\.build.ps1'
	'FarNet.Works.Registry\.build.ps1'
	'FarNetMan\.build.ps1'
)

function Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
	Remove-Item FarNet.sdf -ErrorAction 0
}

task Clean {
	Clean
}

task Install {
	foreach($_ in $Builds) { Invoke-Build Install $_ }
	Copy-Item Far.exe.config $FarHome
	# It may fail in Debug...
	if ($Configuration -eq 'Release') {
		Copy-Item FarNet.Settings\bin\Release\FarNet.Settings.xml, FarNet.Tools\bin\Release\FarNet.Tools.xml $FarHome\FarNet
	}
}

task Uninstall {
	foreach($_ in $Builds) { Invoke-Build Uninstall $_ }
	Remove-Item $FarHome\Far.exe.config -ErrorAction 0
}

task Zip {
	. ..\Get-Version.ps1

	# Build x64
	exec { MSBuild FarNet.sln /t:Build /p:Configuration=Release /p:Platform=x64 }

	# Folders
	Remove-Item [z] -Recurse -Force
	$null = mkdir z\FarNet, z\Plugins\FarNet, z\Plugins.x64\FarNet

	# Root files
	Copy-Item $FarHome\Far.exe.config, Readme.txt, Install.txt z

	# FarNet files
	Copy-Item $FarHome\FarNet\FarNet.* z\FarNet
	Copy-Item $FarHome\Plugins\FarNet\FarNetMan.*, History.txt, LICENSE z\Plugins\FarNet
	Copy-Item FarNetMan\Release\x64\FarNetMan.dll z\Plugins.x64\FarNet

	# Sample modules
	exec { robocopy ..\Modules z\FarNet\Modules /s /np /xf *.suo } 1

	# Zip
	Push-Location z
	exec { & 7z a ..\FarNet.$FarNetVersion.7z * }
	Pop-Location

	Remove-Item z -Recurse -Force
	Clean
}
