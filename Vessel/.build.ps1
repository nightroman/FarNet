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
task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props {
	$Version = Get-Version

	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Product>FarNet.Vessel</Product>
    <Version>$Version</Version>
    <Description>Far Manager smart history of files, folders, commands</Description>
  </PropertyGroup>
</Project>
"@
}

# Build and install the assembly.
task build meta, {
	exec { dotnet build -c Release "/p:FarHome=$FarHome" }
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
	LICENSE,
	Vessel.macro.lua,
	$ModuleHome\Vessel.dll,
	$ModuleHome\Vessel.hlf

	# icon
	Copy-Item ..\Zoo\FarNetLogo.png z
}

task nuget package, version, {
	$description = @'
Vessel is the FarNet module for Far Manager.
It provides smart history of files, folders, commands.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
'@

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
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/Vessel/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}
