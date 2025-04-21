<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'CopyColor'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Copy editor text with colors as HTML. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task clean {
	remove z, bin, obj, README.htm, FarNet.$ModuleName.*.nupkg
}

task version {
	($Script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props version, {
	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Product>FarNet.$ModuleName</Product>
    <Version>$Version</Version>
    <Description>$Description</Description>
  </PropertyGroup>
</Project>
"@
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=README.htm'
			'--from=gfm'
			'--embed-resources'
			'--standalone'
			"--css=$env:MarkdownCss"
			"--metadata=pagetitle:$ModuleName"
		)
	}
}

task package markdown, version, {
	equals "$Version.0" (Get-Item $ModuleRoot\$ModuleName.dll).VersionInfo.FileVersion
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
		"$ModuleRoot\$ModuleName.dll"
	)
}

task nuget package, version, {
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<license type="expression">BSD-3-Clause</license>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/CopyColor</projectUrl>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/CopyColor/History.txt</releaseNotes>
		<tags>FarManager FarNet Module HTML Clipboard</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
