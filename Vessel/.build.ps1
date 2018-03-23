
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$TargetFrameworkVersion = (property TargetFrameworkVersion v3.5)
)

$ModuleHome = "$FarHome\FarNet\Modules\Vessel"

# Synopsis: Build all. Exit Far Manager!
task . Build, Help, Clean

# Get version from release notes.
function Get-Version {
	switch -Regex -File History.txt {'=\s+(\d+\.\d+\.\d+)\s+=' {return $Matches[1]} }
}

# Synopsis: Generate or update meta files.
task Meta -Inputs History.txt, .build.ps1 -Outputs AssemblyInfo.cs {
	$Version = Get-Version

	Set-Content AssemblyInfo.cs @"
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion("$Version")]
[assembly: AssemblyProduct("FarNet.Vessel")]
[assembly: AssemblyTitle("FarNet module Vessel for Far Manager")]
[assembly: AssemblyDescription("FarNet.Vessel: (View/Edit/Save/SELect) file history tools")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) 2011-2017 Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
"@
}

# Build and install the assembly.
task Build Meta, {
	use 14.0 MSBuild
	exec { MSBuild Vessel.csproj /p:Configuration=Release /p:FarHome=$FarHome /p:TargetFrameworkVersion=$TargetFrameworkVersion }
}

# In addition to Build: new About-Vessel.htm, $ModuleHome\Vessel.hlf
task Help {
	exec { MarkdownToHtml "From=About-Vessel.text" "To=About-Vessel.htm" }
	exec { HtmlToFarHelp "From=About-Vessel.htm" "To=$ModuleHome\Vessel.hlf" }
}

task Clean {
	remove z, bin, obj, About-Vessel.htm, FarNet.Vessel.*.nupkg
}

task Version {
	($script:Version = Get-Version)
}

task Package Help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\Vessel'

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	About-Vessel.htm,
	History.txt,
	LICENSE.txt,
	Vessel.macro.lua,
	$ModuleHome\Vessel.dll,
	$ModuleHome\Vessel.hlf
}

task NuGet Package, Version, {
	$text = @'
Vessel (View/Edit/Save/SELect) is the FarNet module for Far Manager.

It records and maintains history of file view, edit, and save operations
and provides related tools.

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
		<id>FarNet.Vessel</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<iconUrl>https://raw.githubusercontent.com/wiki/nightroman/FarNet/images/FarNetLogo.png</iconUrl>
		<licenseUrl>https://raw.githubusercontent.com/nightroman/FarNet/master/Vessel/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://raw.githubusercontent.com/nightroman/FarNet/master/Vessel/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
