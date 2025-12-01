<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	[ValidateSet('Debug', 'Release')]
	[string]$Configuration = 'Release'
	,
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'RedisKit'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Far Manager Redis helpers based on FarNet.Redis'

task build meta, {
	exec { dotnet build -c $Configuration "-p:FarHome=$FarHome" --tl:off }
}

task help -Inputs README.md -Outputs $_root\RedisKit.hlf {
	exec { pandoc.exe $Inputs --output=README.html --from=gfm --syntax-highlighting=none }
	exec { HtmlToFarHelp from=README.html to=$Outputs }
	remove README.html
}

task clean {
	remove z, obj, README.html, *.nupkg
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

task package help, markdown, {
	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\$_name"

	Copy-Item -Destination $toModule `
	$_root\*,
	..\LICENSE,
	README.html,
	History.txt

	Copy-Item ..\Zoo\FarNetLogo.png z
	(Get-Content README.md).Where{!$_.Contains('[Contents]')} | Set-Content z\README.md

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE @'
History.txt
LICENSE
README.html
RedisKit.dll
RedisKit.hlf
RedisKit.pdb
'@
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
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task make_test_tree {
	Import-Module .\Tests\zoo.psm1
	make_test_tree
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
		<tags>FarManager FarNet Redis Client Database</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test {
	exec { pwsf .\Tests -nop -x 999 -c Test-FarNet.ps1 }
}

task . build, help, clean
