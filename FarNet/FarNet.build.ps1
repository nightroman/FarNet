<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	[ValidateScript({"FA::FarNet\FarNetApi.build.ps1", "FM::FarNetMan\FarNetMan.build.ps1"})]
	$Extends,
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

task clean FA::clean, FM::clean, {
	remove z, About-FarNet.html, FarNetTest\bin, FarNetTest\obj
}

task install FA::install, FM::install, help

task uninstall FA::uninstall, FM::uninstall

# Make HLF
task help -If ($Configuration -eq 'Release') {
	exec { pandoc.exe README.md "--output=$env:TEMP\z.html" --from=gfm }
	exec { HtmlToFarHelp.exe "from=$env:TEMP\z.html" "to=$FarHome\Plugins\FarNet\FarNetMan.hlf" }
}

# Make markdown
task markdown {
	requires -Path $env:MarkdownCss
	exec {
		pandoc.exe @(
			'README.md'
			'--output=About-FarNet.html'
			'--from=gfm'
			'--embed-resources'
			'--standalone'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

# Synopsis: Make Win32 files.
task buildWin32 {
	equals $Platform x64

	#! build just FarNetMan
	exec { & (Resolve-MSBuild) @(
		"..\FarNet.slnx"
		"/t:restore,FarNetMan"
		"/p:Platform=Win32"
		"/p:FarHome=$(Split-Path $FarHome)\Win32"
		"/p:Configuration=Release"
	)}
}

# Make package files
task package buildWin32, markdown, {
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
		'About-FarNet.html'
		'History.txt'
		'..\LICENSE'
		"$FarHome\FarNet\FarNet.dll"
		"$FarHome\FarNet\FarNet.xml"
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
	Get-Content ..\README.md | ?{$_ -notlike '*FarNetLogo.png*'} | Set-Content z\README.md

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<description>.NET API for Far Manager and runtime for .NET modules and scripts.</description>
		<id>FarNet</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/FarNet/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
		<readme>README.md</readme>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task test {
	Set-Location FarNetTest
	Use-BuildEnv @{FarNetTest = 1} {
		exec { dotnet run }
	}
	remove bin, obj
}
