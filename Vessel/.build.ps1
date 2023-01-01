<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'Vessel'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Enhanced history of files, folders, commands. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	Copy-Item -Destination $ModuleRoot @(
		"bin\$Configuration\net7.0\$ModuleName.dll"
		"bin\$Configuration\net7.0\$ModuleName.pdb"
	)
}

task clean {
	remove z, bin, obj, README.htm, FarNet.$ModuleName.*.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
	assert $script:Version
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
	# HLF
	exec { pandoc.exe README.md --output=README.htm --from=gfm }
	exec { HtmlToFarHelp "from=README.htm" "to=$ModuleRoot\Vessel.hlf" }

	# HTM
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=README.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			"--metadata=pagetitle:$ModuleName"
		)
	}
}

task package markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# main
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
		'Vessel.macro.lua'
		"$ModuleRoot\Vessel.dll"
		"$ModuleRoot\Vessel.hlf"
	)

	# meta
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

task . build, markdown, clean
