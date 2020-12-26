<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

$ModuleHome = "$FarHome\FarNet\Modules\Vessel"

# Synopsis: Build all. Exit Far Manager!
task . build, help, clean

# Get version from release notes.
function Get-Version {
	switch -Regex -File History.txt {'=\s+(\d+\.\d+\.\d+)\s+=' {return $Matches[1]} }
}

# Synopsis: Generate or update meta files.
task meta -Inputs History.txt, .build.ps1 -Outputs AssemblyInfo.cs {
	$Version = Get-Version

	Set-Content AssemblyInfo.cs @"
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion("$Version")]
[assembly: AssemblyProduct("FarNet.Vessel")]
[assembly: AssemblyTitle("FarNet module Vessel for Far Manager")]
[assembly: AssemblyDescription("FarNet.Vessel - smart history of files, folders, commands")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
"@
}

# Build and install the assembly.
task build meta, {
	exec { & (Resolve-MSBuild) Vessel.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# In addition to Build: new About-Vessel.htm, $ModuleHome\Vessel.hlf
task help {
	# HLF
	exec { pandoc.exe README.md --output=About-Vessel.htm --from=gfm }
	exec { HtmlToFarHelp "from=About-Vessel.htm" "to=$ModuleHome\Vessel.hlf" }

	# HTM
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=About-Vessel.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:Vessel'
		)
	}
}

task clean {
	remove z, bin, obj, About-Vessel.htm, FarNet.Vessel.*.nupkg
}

task version {
	($script:Version = Get-Version)
}

task package help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\Vessel'

	remove z
	$null = mkdir $toModule

	# main
	Copy-Item -Destination $toModule `
	About-Vessel.htm,
	History.txt,
	LICENSE.txt,
	Vessel.macro.lua,
	$ModuleHome\Vessel.dll,
	$ModuleHome\Vessel.hlf

	# icon
	Copy-Item ..\Zoo\FarNetLogo.png z
}

task nuget package, version, {
	$text = @'
Vessel is the FarNet module for Far Manager.
It provides smart history of files, folders, commands.

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
		<id>FarNet.Vessel</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/Vessel/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
