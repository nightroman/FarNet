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
$ModuleName = 'RedisKit'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Far Manager Redis helpers based on FarNet.Redis'

task build meta, {
	exec { dotnet build -c $Configuration "-p:FarHome=$FarHome" }
}

task help -Inputs README.md -Outputs $ModuleRoot\RedisKit.hlf {
	exec { pandoc.exe $Inputs --output=README.htm --from=gfm --no-highlight }
	exec { HtmlToFarHelp from=README.htm to=$Outputs }
	remove README.htm
}

task clean {
	remove z, obj, README.htm, *.nupkg
}

task version {
	($Script:Version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		"--metadata=pagetitle=$ModuleName"
	)}
}

task package help, markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } 1

	# meta
	Copy-Item ..\Zoo\FarNetLogo.png z
	(Get-Content README.md).Where{!$_.Contains('[Contents]')} | Set-Content z\README.md

	# repo
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile.ps1 -Result (Get-ChildItem z\tools -Recurse -File -Name) -Text -View $env:MERGE @'
FarHome\FarNet\Modules\RedisKit\History.txt
FarHome\FarNet\Modules\RedisKit\LICENSE
FarHome\FarNet\Modules\RedisKit\README.htm
FarHome\FarNet\Modules\RedisKit\RedisKit.dll
FarHome\FarNet\Modules\RedisKit\RedisKit.hlf
'@
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
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

task make_test_tree {
	Import-Module .\Tests\zoo.psm1
	make_test_tree
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
		<tags>FarManager FarNet Redis Client Database</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test {
	Start-Far "ps: Test-FarNet * -Quit" .\Tests -ReadOnly
}

task . build, help, clean
