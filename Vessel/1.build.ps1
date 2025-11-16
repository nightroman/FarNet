<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'Vessel'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Enhanced history of files, folders, commands. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome --tl:off }
}

task clean {
	remove z, bin, obj, README.html, FarNet.$_name.*.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task meta -Inputs 1.build.ps1, History.txt -Outputs Directory.Build.props version, {
	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Product>FarNet.$_name</Product>
    <Version>$_version</Version>
    <Description>$_description</Description>
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
		'--standalone'
		'--embed-resources'
		"--css=$env:MarkdownCss"
		'--metadata=lang:en'
		"--metadata=pagetitle:$_name"
	)}
	exec { HtmlToFarHelp.exe "from=README.html" "to=$_root\Vessel.hlf" }
}

task package markdown, {
	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\$_name"

	Copy-Item -Destination $toModule `
	$_root\*,
	..\LICENSE,
	README.html,
	History.txt,
	Vessel.macro.lua

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE @'
History.txt
LICENSE
README.html
Vessel.dll
Vessel.hlf
Vessel.macro.lua
Vessel.pdb
'@
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

task . build, markdown, clean
