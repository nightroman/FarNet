
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"

use 4.0 MSBuild

$script:Builds = @(
	'FarNet\.build.ps1'
	'FarNet.Settings\.build.ps1'
	'FarNet.Tools\.build.ps1'
	'FarNet.Works.Config\.build.ps1'
	'FarNet.Works.Dialog\.build.ps1'
	'FarNet.Works.Editor\.build.ps1'
	'FarNet.Works.Manager\.build.ps1'
	'FarNet.Works.Panels\.build.ps1'
	'FarNetMan\.build.ps1'
)

function Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
	Remove-Item -Force -Recurse -ErrorAction 0 -LiteralPath z, FarNet.sdf, About-FarNet.htm
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
},
Help

task Uninstall {
	foreach($_ in $Builds) { Invoke-Build Uninstall $_ }
	Remove-Item $FarHome\Far.exe.config -ErrorAction 0
}

task Help -If ($Configuration -eq 'Release') {
	exec { MarkdownToHtml "From=About-FarNet.text" "To=About-FarNet.htm" }
	exec { HtmlToFarHelp "From=About-FarNet.htm" "To=$FarHome\Plugins\FarNet\FarNetMan.hlf" }
}

# Make package files
task Package Help, {
	assert ($Platform -eq 'Win32')

	# build x64
	exec { MSBuild FarNet.sln /t:Build /p:Configuration=Release /p:Platform=x64 }

	# folders
	Remove-Item [z] -Recurse -Force
	$null = mkdir `
	z\tools\FarHome\FarNet,
	z\tools\FarHome\Plugins\FarNet,
	z\tools\FarHome.x64\Plugins\FarNet,
	z\tools\FarHome.x86\Plugins\FarNet

	# copy
	Copy-Item -Destination z\tools About-FarNet.htm, History.txt, $FarHome\FarNet\FarNetAPI.chm
	Copy-Item -Destination z\tools\FarHome $FarHome\Far.exe.config
	Copy-Item -Destination z\tools\FarHome\FarNet $FarHome\FarNet\FarNet.*
	Copy-Item -Destination z\tools\FarHome\Plugins\FarNet $FarHome\Plugins\FarNet\FarNetMan.hlf, LICENSE.txt
	Copy-Item -Destination z\tools\FarHome.x64\Plugins\FarNet FarNetMan\Release\x64\FarNetMan.dll
	Copy-Item -Destination z\tools\FarHome.x86\Plugins\FarNet $FarHome\Plugins\FarNet\FarNetMan.dll

	# samples
	exec { robocopy ..\Modules z\tools\Samples /s /np /xf *.suo, Modules.build.ps1 } 1
}

# Set version
task Version {
	. ..\Get-Version.ps1
	$script:Version = $FarNetVersion
	$Version
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
FarNet provides the .NET API for Far Manager and the runtime infrastructure for
.NET modules. The package includes the framework and the module manager plugin.

---

To install and update FarNet packages, follow these steps:

https://farnet.googlecode.com/svn/trunk/Install-FarNet.en.txt

---
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/FarNetLogo.png</iconUrl>
		<licenseUrl>http://opensource.org/licenses/BSD-3-Clause</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/FarNet/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
