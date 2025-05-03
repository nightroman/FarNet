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
$ModuleName = 'GitKit'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Far Manager git helpers based on LibGit2Sharp.'

task build meta, {
	exec { dotnet build -c $Configuration "-p:FarHome=$FarHome" }
}

task publish {
	$xml = [xml](Get-Content "$ModuleName.csproj" -Raw)
	$ver1 = $xml.SelectSingleNode('//PackageReference[@Include="LibGit2Sharp"]').Version
	$ver2 = $xml.SelectSingleNode('//PackageReference[@Include="LibGit2Sharp.NativeBinaries"]').Version

	$bit = if ($FarHome -match 'x64') {'win-x64'} elseif ($FarHome -match 'Win32') {'win-x86'} else {throw}
	Copy-Item -Destination $ModuleRoot @(
		"$HOME\.nuget\packages\LibGit2Sharp\$ver1\lib\net8.0\LibGit2Sharp.dll"
		"$HOME\.nuget\packages\LibGit2Sharp\$ver1\lib\net8.0\LibGit2Sharp.xml"
		"$HOME\.nuget\packages\LibGit2Sharp.NativeBinaries\$ver2\runtimes\$bit\native\*.dll"
	)
}

task help -Inputs README.md -Outputs $ModuleRoot\$ModuleName.hlf {
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

task win32 {
	Invoke-Build build -FarHome C:\Bin\Far\Win32
}

task package win32, help, markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$toModule64 = mkdir "z\tools\FarHome.x64\FarNet\Modules\$ModuleName"
	$toModule86 = mkdir "z\tools\FarHome.x86\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb git2*.dll} (0..2)

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# repo
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
	)

	# bits
	Copy-Item $ModuleRoot\git2*.dll $toModule64
	Copy-Item C:\Bin\Far\Win32\FarNet\Modules\GitKit\git2*.dll $toModule86

	Assert-SameFile.ps1 -Result (Get-ChildItem z\tools -Recurse -File -Name) -Text -View $env:MERGE @'
FarHome\FarNet\Modules\GitKit\GitKit.dll
FarHome\FarNet\Modules\GitKit\GitKit.hlf
FarHome\FarNet\Modules\GitKit\History.txt
FarHome\FarNet\Modules\GitKit\LibGit2Sharp.dll
FarHome\FarNet\Modules\GitKit\LibGit2Sharp.xml
FarHome\FarNet\Modules\GitKit\LICENSE
FarHome\FarNet\Modules\GitKit\README.htm
FarHome.x64\FarNet\Modules\GitKit\git2-3f4182d.dll
FarHome.x86\FarNet\Modules\GitKit\git2-3f4182d.dll
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
		<tags>FarManager FarNet Module Git</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test {
	Start-Far "ps: ..\..\Test\Test-FarNet.ps1 * -Quit" .\Tests -ReadOnly
}

task . build, help, clean
