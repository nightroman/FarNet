<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'RightWords'
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	exec { dotnet publish "$ModuleName.csproj" -c $Configuration -o $ModuleHome --no-build }
	Remove-Item -LiteralPath "$ModuleHome\$ModuleName.deps.json"
},
help,
resgen

task help @{
	Inputs = 'README.md'
	Outputs = "$ModuleHome\RightWords.hlf"
	Jobs = {
		exec { pandoc.exe README.md --output=z.htm --from=gfm }
		exec { HtmlToFarHelp from=z.htm to=$ModuleHome\RightWords.hlf }
		remove z.htm
	}
}

# https://github.com/nightroman/PowerShelf/blob/master/Invoke-Environment.ps1
task resgen @{
	Inputs = 'RightWords.restext', 'RightWords.ru.restext'
	Outputs = "$ModuleHome\RightWords.resources", "$ModuleHome\RightWords.ru.resources"
	Partial = $true
	Jobs = {
		begin {
			$VsDevCmd = @(Get-Item "$env:ProgramFiles\Microsoft Visual Studio\2022\*\Common7\Tools\VsDevCmd.bat")
			Invoke-Environment.ps1 -File ($VsDevCmd[0])
		}
		process {
			exec {resgen.exe $_ $2}
		}
	}
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
	$dll = Get-Item "$ModuleHome\RightWords.dll"
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	equals ($matches[1]) $script:Version

	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule @(
		"README.htm"
		"History.txt"
		"LICENSE"
		"RightWords.macro.lua"
		"$ModuleHome\RightWords.dll"
		"$ModuleHome\RightWords.hlf"
		"$ModuleHome\RightWords.resources"
		"$ModuleHome\RightWords.ru.resources"
		"$ModuleHome\WeCantSpell.Hunspell.dll"
	)
}

task nuget package, version, {
	$text = @'
FarNet module for Far Manager, spell-checker.

The spell-checker is based on Hunspell used in OpenOffice.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme

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
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/RightWords/History.txt</releaseNotes>
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
[assembly: AssemblyDescription("Spell-checker")]
[assembly: AssemblyProduct("FarNet.RightWords")]
[assembly: AssemblyTitle("FarNet module RightWords for Far Manager")]
[assembly: AssemblyVersion("$script:Version")]
"@
}

task . build, clean
