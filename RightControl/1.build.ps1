<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'RightControl'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Editor and line editor tweaks. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome --tl:off }
}

task clean {
	remove z, bin, obj, README.html, FarNet.$_name.*.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task meta -Inputs $BuildFile, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Description>$_description</Description>
    <Product>FarNet.$_name</Product>
    <Version>$_version</Version>
    <IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
  </PropertyGroup>
</Project>
"@
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=README.html'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		'--metadata=pagetitle:FarNet'
	)}
}

task package markdown, version, {
	equals $_version (Get-Item $_root\$_name.dll).VersionInfo.ProductVersion

	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\$_name"

	# module
	Copy-Item -Destination $toModule `
	$_root\*,
	..\LICENSE,
	README.html,
	History.txt,
	RightControl.macro.lua

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)
}

task nuget package, version, {
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$_name</id>
		<version>$_version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$_name/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
