<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'RightControl'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Editor and line editor tweaks. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	remove "$ModuleRoot\$ModuleName.deps.json"
}

task clean {
	remove z, bin, obj, README.htm, FarNet.$ModuleName.*.nupkg
}

task version {
	($script:Version = switch -Regex -File History.txt {'=\s*(\d+\.\d+\.\d+)\s*=' {$Matches[1]; break}})
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Description>$Description</Description>
    <Product>FarNet.$ModuleName</Product>
    <Version>$Version</Version>
    <IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
  </PropertyGroup>
</Project>
"@
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		'--metadata=pagetitle:FarNet'
	)}
}

task package markdown, version, {
	equals $Version (Get-Item $ModuleRoot\$ModuleName.dll).VersionInfo.ProductVersion
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
		"$ModuleRoot\$ModuleName.dll"
		'RightControl.macro.lua'
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
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
