<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64)
)

$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\RightControl"

# Get version from history.
function Get-Version {
	switch -Regex -File History.txt {'=\s*(\d+\.\d+\.\d+)\s*=' {return $Matches[1]} }
}

# Generate or update meta files.
task meta -Inputs History.txt -Outputs AssemblyInfo.cs {
	$Version = Get-Version

	Set-Content AssemblyInfo.cs @"
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("FarNet.RightControl")]
[assembly: AssemblyVersion("$Version")]
[assembly: AssemblyTitle("FarNet module RightControl for Far Manager")]
[assembly: AssemblyDescription("Some editor actions work like in other editors")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
"@
}

# Build and install
task build meta, {
	$MSBuild = Resolve-MSBuild
	exec { & $MSBuild RightControl.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# New About-RightControl.htm
task help {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=About-RightControl.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

# Remove temp files
task clean {
	remove z, bin, obj, AssemblyInfo.cs, About-RightControl.htm, FarNet.RightControl.*.nupkg
}

# Set $script:Version
task version {
	($script:Version = Get-Version)
}

# Copy package files to z\tools
task package help, version, {
	equals "$Version.0" (Get-Item $ModuleHome\RightControl.dll).VersionInfo.FileVersion
	$toModule = 'z\tools\FarHome\FarNet\Modules\RightControl'

	remove z
	$null = mkdir $toModule

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule `
	About-RightControl.htm,
	History.txt,
	LICENSE.txt,
	RightControl.macro.lua,
	$ModuleHome\RightControl.dll
}

# New NuGet package
task nuget package, version, {
	$text = @'
RightControl is the FarNet module for Far Manager.

It alters some actions in editors, edit controls, and the command line.
New actions are similar to what many popular editors do on stepping,
selecting, deleting by words, and etc.

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
		<id>FarNet.RightControl</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/RightControl/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
