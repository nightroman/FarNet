<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

$ModuleName = 'RightControl'
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

# Get version from history.
function Get-Version {
	switch -Regex -File History.txt {'=\s*(\d+\.\d+\.\d+)\s*=' {return $Matches[1]} }
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props {
	$Version = Get-Version

	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Product>FarNet.$ModuleName</Product>
    <Version>$Version</Version>
    <Description>Far Manager editor and line editor tweaks</Description>
  </PropertyGroup>
</Project>
"@
}

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	Copy-Item -Destination $ModuleHome @(
		"bin\$Configuration\net6.0\$ModuleName.dll"
		"bin\$Configuration\net6.0\$ModuleName.pdb"
	)
}

task help {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=README.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

task clean {
	remove z, bin, obj, README.htm, FarNet.$ModuleName.*.nupkg
}

task version {
	($script:Version = Get-Version)
}

task package help, version, {
	equals "$Version.0" (Get-Item $ModuleHome\$ModuleName.dll).VersionInfo.FileVersion
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE'
		"$ModuleHome\$ModuleName.dll"
		'RightControl.macro.lua'
	)
}

task nuget package, version, {
	$description = @'
RightControl is the FarNet module for Far Manager.

It alters some actions in editors, edit controls, and the command line.
New actions are similar to what many popular editors do on stepping,
selecting, deleting by words, and etc.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
'@

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
