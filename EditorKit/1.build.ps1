<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

Set-StrictMode -Version 3
$_name = 'EditorKit'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Editor configuration. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build "$_name.csproj" "/p:FarHome=$FarHome" "/p:Configuration=$Configuration" }
}

task clean {
	remove z, bin, obj, README.html, Directory.Build.props, "*$_name.*.nupkg"
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
		"--metadata=pagetitle=$_name"
	)}
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

task package markdown, {
	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\$_name"

	Copy-Item -Destination $toModule `
	$_root\*,
	..\LICENSE,
	README.html,
	History.txt

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE @'
EditorConfig.Core.dll
EditorKit.dll
EditorKit.pdb
History.txt
LICENSE
README.html
'@
}

task nuget package, version, {
	equals $_version (Get-Item "$_root\$_name.dll").VersionInfo.ProductVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$_name</id>
		<version>$_version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/$_name</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$_name/History.txt</releaseNotes>
		<tags>FarManager FarNet Module EditorConfig</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
