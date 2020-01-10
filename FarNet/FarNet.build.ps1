
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release),
	$TargetFrameworkVersion = (property TargetFrameworkVersion v3.5)
)
$FarHome = "C:\Bin\Far\$Platform"

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
	remove z, FarNet.sdf, About-FarNet.htm
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
	remove $FarHome\Far.exe.config
}

# HLF
task Help -If ($Configuration -eq 'Release') {
	exec { pandoc.exe About-FarNet.text --output=About-FarNet.htm --from=markdown_phpextra }
	exec { HtmlToFarHelp from=About-FarNet.htm to=$FarHome\Plugins\FarNet\FarNetMan.hlf }
}

# HTM
task HelpHtm {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'About-FarNet.text'
			'--output=About-FarNet.htm'
			'--from=markdown_phpextra'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

# HLF and HTM
task Help2 Help, HelpHtm

# Tests before packaging
task BeginPackage {
	# Far.exe.config
	$xml = [xml](Get-Content $FarHome\Far.exe.config)
	$nodes = @($xml.SelectNodes('configuration/appSettings/add'))
	assert ($nodes.Count -eq 0)
	$nodes = @($xml.SelectNodes('configuration/system.diagnostics/switches/add'))
	assert ($nodes.Count -eq 1)
	assert ($nodes[0].name -ceq 'FarNet.Trace')
	assert ($nodes[0].value -ceq 'Warning')
}

# Make package files
task Package BeginPackage, Help2, {
	Set-Alias MSBuild (Resolve-MSBuild)
	# build another platform
	$bit = if ($Platform -eq 'Win32') {'x64'} else {'Win32'}
	$PlatformToolset = if ($TargetFrameworkVersion -lt 'v4') {'v90'} else {'v140'}
	exec {
		MSBuild @(
			"..\FarNetAccord.sln"
			"/t:FarNetMan"
			"/p:Platform=$bit"
			"/p:Configuration=Release"
			"/p:TargetFrameworkVersion=$TargetFrameworkVersion"
			"/p:PlatformToolset=$PlatformToolset"
		)
	}

	# folders
	remove z
	$null = mkdir `
	z\tools\FarHome\FarNet,
	z\tools\FarHome\Plugins\FarNet,
	z\tools\FarHome.x64\Plugins\FarNet,
	z\tools\FarHome.x86\Plugins\FarNet

	# copy
	[System.IO.File]::Delete("$FarHome\FarNet\FarNetAPI.chw")
	Copy-Item -Destination z\tools\FarHome $FarHome\Far.exe.config
	Copy-Item -Destination z\tools\FarHome\FarNet $(
		'About-FarNet.htm'
		'History.txt'
		'LICENSE.txt'
		"$FarHome\FarNet\FarNet.dll"
		"$FarHome\FarNet\FarNet.xml"
		"$FarHome\FarNet\FarNet.Settings.dll"
		"$FarHome\FarNet\FarNet.Settings.xml"
		"$FarHome\FarNet\FarNet.Tools.dll"
		"$FarHome\FarNet\FarNet.Tools.xml"
		"$FarHome\FarNet\FarNet.Works.Config.dll"
		"$FarHome\FarNet\FarNet.Works.Dialog.dll"
		"$FarHome\FarNet\FarNet.Works.Editor.dll"
		"$FarHome\FarNet\FarNet.Works.Manager.dll"
		"$FarHome\FarNet\FarNet.Works.Panels.dll"
		"$FarHome\FarNet\FarNetAPI.chm"
	)
	Copy-Item -Destination z\tools\FarHome\Plugins\FarNet $FarHome\Plugins\FarNet\FarNetMan.hlf
	if ($Platform -eq 'Win32') {
		Copy-Item -Destination z\tools\FarHome.x64\Plugins\FarNet FarNetMan\Release\x64\FarNetMan.dll
		Copy-Item -Destination z\tools\FarHome.x86\Plugins\FarNet $FarHome\Plugins\FarNet\FarNetMan.dll
	}
	else {
		Copy-Item -Destination z\tools\FarHome.x64\Plugins\FarNet $FarHome\Plugins\FarNet\FarNetMan.dll
		Copy-Item -Destination z\tools\FarHome.x86\Plugins\FarNet FarNetMan\Release\Win32\FarNetMan.dll
	}

	# icon
	$null = mkdir z\images
	Copy-Item ..\Zoo\FarNetLogo.png z\images
}

# Set version
task Version {
	. ..\Get-Version.ps1
	($script:Version = $FarNetVersion)
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
FarNet provides the .NET API for Far Manager and the runtime infrastructure for
.NET modules. The package includes the framework and the module manager plugin.

---

To install FarNet packages, follow these steps:

https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt

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
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>images\FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://raw.githubusercontent.com/nightroman/FarNet/master/FarNet/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
