<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'CopyColor'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Copy editor text with colors as HTML. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task clean {
	remove z, bin, obj, README.html, FarNet.$_name.*.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task meta -Inputs $BuildFile, History.txt -Outputs Directory.Build.props version, {
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
	exec {
		pandoc.exe @(
			'README.md'
			'--output=README.html'
			'--from=gfm'
			'--embed-resources'
			'--standalone'
			"--css=$env:MarkdownCss"
			"--metadata=pagetitle:$_name"
		)
	}
}

task package markdown, version, {
	equals "$_version.0" (Get-Item $_root\$_name.dll).VersionInfo.FileVersion
	$toModule = "z\tools\FarHome\FarNet\Modules\$_name"

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Copy-Item -Destination $toModule @(
		'README.html'
		'History.txt'
		'..\LICENSE'
		"$_root\$_name.dll"
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
		<license type="expression">BSD-3-Clause</license>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/CopyColor</projectUrl>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/CopyColor/History.txt</releaseNotes>
		<tags>FarManager FarNet Module HTML Clipboard</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
