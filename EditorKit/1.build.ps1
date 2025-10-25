<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

Set-StrictMode -Version 3
$ModuleName = 'EditorKit'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Editor configuration. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build "$ModuleName.csproj" "/p:FarHome=$FarHome" "/p:Configuration=$Configuration" }
}

task clean {
	remove z, bin, obj, README.html, Directory.Build.props, "*$ModuleName.*.nupkg"
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
		"--metadata=pagetitle=$ModuleName"
	)}
}

task version {
	($Script:Version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task meta -Inputs $BuildFile, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>$Description</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.$ModuleName</Product>
		<Version>$Version</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task package markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } (0..2)

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# module
	Copy-Item -Destination $toModule @(
		'README.html'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE @'
EditorConfig.Core.dll
EditorKit.dll
History.txt
LICENSE
README.html
'@
}

task nuget package, version, {
	equals $Version (Get-Item "$ModuleRoot\$ModuleName.dll").VersionInfo.ProductVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/$ModuleName</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module EditorConfig</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
