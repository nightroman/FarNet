<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'FolderChart'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Shows folder sizes chart. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome --tl:off }
}

task clean {
	remove z, bin, obj, README.html, *.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
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

task meta -Inputs 1.build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>$_description</Description>
		<Product>FarNet.$_name</Product>
		<Version>$_version</Version>
		<FileVersion>$_version</FileVersion>
		<AssemblyVersion>$_version</AssemblyVersion>
	</PropertyGroup>
</Project>
"@
}

task package version, markdown, {
	equals $_version (Get-Item "$_root\$_name.dll").VersionInfo.FileVersion

	remove z
	$toModule = New-Item "z\tools\FarHome\FarNet\Modules\$_name" -ItemType Directory

	Copy-Item "$_root\*" $toModule -Recurse

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Copy-Item -Destination $toModule @(
		"README.html"
		"History.txt"
		"..\LICENSE"
	)

	Assert-SameFile.ps1 -Text -View $env:MERGE -Result (Get-ChildItem $toModule -Recurse -File -Name) -Sample @'
FolderChart.dll
FolderChart.pdb
History.txt
LICENSE
README.html
System.Windows.Forms.DataVisualization.dll
'@
}

task nuget package, {
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.FolderChart</id>
		<version>$_version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/FolderChart/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
