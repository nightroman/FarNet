<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64)
)

$FarHome = "C:\Bin\Far\$Platform"
$fromModule = "$FarHome\FarNet\Modules\RightWords"
$fromNHunspell = "$FarHome\FarNet\NHunspell"

task build meta, {
	exec { & (Resolve-MSBuild) $(
		"RightWords.csproj"
		"/p:FarHome=$FarHome"
		"/p:Configuration=Release"
	)}
}

task clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--self-contained', "--css=$env:MarkdownCss"
		'--standalone', '--metadata=pagetitle=RightWords'
	)}
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
	assert $script:Version
}

task package markdown, version, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\RightWords'
	$toNHunspell = 'z\tools\FarHome\FarNet\NHunspell'

	$dll = Get-Item "$fromModule\RightWords.dll"
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	equals ($matches[1]) $script:Version

	remove z
	$null = mkdir $toModule, $toNHunspell

	# package: logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# FarNet\NHunspell
	Copy-Item -Destination $toNHunspell $(
		"$fromNHunspell\Hunspellx64.dll"
		"$fromNHunspell\Hunspellx86.dll"
		"$fromNHunspell\NHunspell.dll"
	)

	# FarNet\Modules\RightWords
	Copy-Item -Destination $toModule $(
		"README.htm"
		"History.txt"
		"LICENSE.txt"
		"RightWords.macro.lua"
		"$fromModule\RightWords.dll"
		"$fromModule\RightWords.resources"
		"$fromModule\RightWords.ru.resources"
	)
}

task nuget package, version, {
	$text = @'
FarNet module for Far Manager, spell-checker and thesaurus.

It provides the spell-checker and thesaurus based on NHunspell. The core
Hunspell is used in OpenOffice and it works with dictionaries published
on OpenOffice.org.

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
		<id>FarNet.RightWords</id>
		<version>$script:Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://raw.githubusercontent.com/nightroman/FarNet/master/RightWords/History.txt</releaseNotes>
		<tags>FarManager FarNet Module NHunspell</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task meta -Inputs .build.ps1, History.txt -Outputs AssemblyInfo.cs -Jobs version, {
	Set-Content AssemblyInfo.cs @"
using System.Reflection;
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")]
[assembly: AssemblyDescription("Spell-checker and thesaurus")]
[assembly: AssemblyProduct("FarNet.RightWords")]
[assembly: AssemblyTitle("FarNet module RightWords for Far Manager")]
[assembly: AssemblyVersion("$script:Version")]
"@
}

task . build, clean
