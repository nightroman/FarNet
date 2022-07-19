<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release),
	$TargetFramework = (property TargetFramework net6.0)
)
$FarHome = "C:\Bin\Far\$Platform"

$script:Builds = @(
	'FarNet\.build.ps1'
	'FarNet.Works.Config\.build.ps1'
	'FarNet.Works.Dialog\.build.ps1'
	'FarNet.Works.Editor\.build.ps1'
	'FarNet.Works.Loader\.build.ps1'
	'FarNet.Works.Panels\.build.ps1'
	'FarNetMan\.build.ps1'
)

function do-clean {
	foreach($_ in $Builds) { Invoke-Build clean $_ }
	remove z, FarNet.sdf, About-FarNet.htm
}

task clean {
	do-clean
}

task install {
	foreach($_ in $Builds) {
		Invoke-Build install $_
	}
},
helpHLF

task uninstall {
	foreach($_ in $Builds) { Invoke-Build uninstall $_ }
}

# Make HLF, called by Build (Install), depends on x64/x86
task helpHLF -If ($Configuration -eq 'Release') {
	exec { pandoc.exe README.md --output=z.htm --from=gfm }
	exec { HtmlToFarHelp from=z.htm to=$FarHome\Plugins\FarNet\FarNetMan.hlf }
	remove z.htm
}

# Make HTM
task helpHTM {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=About-FarNet.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

# Test config and make another platform before packaging
task beginPackage {
	# make another platform
	$bit = if ($Platform -eq 'Win32') {'x64'} else {'Win32'}

	#! build just FarNetMan, PowerShellFar is not needed and causes locked files...
	exec { & (Resolve-MSBuild) @(
		"..\FarNet6.sln"
		"/t:restore,FarNetMan"
		"/p:Platform=$bit"
		"/p:Configuration=Release"
	)}
}

# Make package files
task package beginPackage, helpHTM, {
	# folders
	remove z
	$null = mkdir `
	z\tools\FarHome\FarNet,
	z\tools\FarHome\Plugins\FarNet,
	z\tools\FarHome.x64\Plugins\FarNet,
	z\tools\FarHome.x86\Plugins\FarNet

	# copy
	[System.IO.File]::Delete("$FarHome\FarNet\FarNetAPI.chw")
	Copy-Item -Destination z\tools\FarHome\FarNet $(
		'About-FarNet.htm'
		'History.txt'
		'..\LICENSE'
		"$FarHome\FarNet\FarNet.dll"
		"$FarHome\FarNet\FarNet.xml"
		"$FarHome\FarNet\FarNet.Works.Config.dll"
		"$FarHome\FarNet\FarNet.Works.Dialog.dll"
		"$FarHome\FarNet\FarNet.Works.Editor.dll"
		"$FarHome\FarNet\FarNet.Works.Loader.dll"
		"$FarHome\FarNet\FarNet.Works.Panels.dll"
		"$FarHome\FarNet\FarNetAPI.chm"
	)
	Copy-Item -Destination z\tools\FarHome\Plugins\FarNet @(
		"$FarHome\Plugins\FarNet\FarNetMan.hlf"
		"$FarHome\Plugins\FarNet\FarNetMan.runtimeconfig.json"
	)
	if ($Platform -eq 'Win32') {
		Copy-Item -Destination z\tools\FarHome.x64\Plugins\FarNet @(
			"FarNetMan\Release\x64\FarNetMan.dll"
			"FarNetMan\Release\x64\Ijwhost.dll"
		)
		Copy-Item -Destination z\tools\FarHome.x86\Plugins\FarNet @(
			"$FarHome\Plugins\FarNet\FarNetMan.dll"
			"$FarHome\Plugins\FarNet\Ijwhost.dll"
		)
	}
	else {
		Copy-Item -Destination z\tools\FarHome.x64\Plugins\FarNet @(
			"$FarHome\Plugins\FarNet\FarNetMan.dll"
			"$FarHome\Plugins\FarNet\Ijwhost.dll"
		)
		Copy-Item -Destination z\tools\FarHome.x86\Plugins\FarNet @(
			"FarNetMan\Release\Win32\FarNetMan.dll"
			"FarNetMan\Release\Win32\Ijwhost.dll"
		)
	}

	# icon
	Copy-Item ..\Zoo\FarNetLogo.png z
}

# Set version
task version {
	. ..\Get-Version.ps1
	($script:Version = $FarNetVersion)
}

# Make NuGet package
task nuget package, version, {
	$description = @'
FarNet provides the .NET API for Far Manager and the runtime infrastructure for
.NET modules. The package includes the framework and the module manager plugin.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
'@

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/FarNet/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}
